using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using Domain.Model.ApiResponses; // Para RecetaResponse
using Microsoft.Maui.Storage; // Para SecureStorage
using System;
using Refit; // Para ApiException

namespace proyectoFin.MVVM.ViewModel
{
    public partial class FavoritesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaResponse> favoriteRecipes;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private bool _showNoFavoritesMessage;


        public IAsyncRelayCommand LoadFavoriteRecipesCommand { get; }
        public IAsyncRelayCommand<RecetaResponse> SelectFavoriteRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;

        public FavoritesViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            FavoriteRecipes = new ObservableCollection<RecetaResponse>();
            FavoriteRecipes.CollectionChanged += (s, e) => ShowNoFavoritesMessage = FavoriteRecipes.Count == 0;

            LoadFavoriteRecipesCommand = new AsyncRelayCommand(LoadFavoriteRecipesAsync);
            SelectFavoriteRecipeCommand = new AsyncRelayCommand<RecetaResponse>(OnFavoriteRecipeSelected);

            _ = LoadFavoriteRecipesAsync();
        }

        private async Task LoadFavoriteRecipesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            ShowNoFavoritesMessage = false;

            string token = await SecureStorage.GetAsync("jwt_token");
            int retryCount = 0;
            const int maxRetries = 3;
            const int retryDelayMs = 500;

            if (string.IsNullOrEmpty(token))
            {
                while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                {
                    Console.WriteLine($"DEBUG: Token no encontrado aún en FavoritesViewModel. Reintentando... (Intento {retryCount + 1})");
                    await Task.Delay(retryDelayMs);
                    token = await SecureStorage.GetAsync("jwt_token");
                    retryCount++;
                }
            }

            if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
            {
                ErrorMessage = "No se pudo obtener el token de autenticación para cargar favoritos. Por favor, intente iniciar sesión de nuevo.";
                await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                IsBusy = false;
                ShowNoFavoritesMessage = true;
                return;
            }

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado para cargar favoritos.";
                    IsBusy = false;
                    ShowNoFavoritesMessage = true;
                    return;
                }

                // ¡CORRECCIÓN CLAVE AQUÍ! Llamada al método existente en IBakeOrTakeApi
                var response = await _apiService.GetFavoritosByClientAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    FavoriteRecipes.Clear();
                    foreach (var recetaResponse in response.Content)
                    {
                        FavoriteRecipes.Add(recetaResponse);
                    }
                    ShowNoFavoritesMessage = FavoriteRecipes.Count == 0;
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar las recetas favoritas. Código: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                    ShowNoFavoritesMessage = true;
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
                ShowNoFavoritesMessage = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar las recetas favoritas: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnFavoriteRecipeSelected(RecetaResponse selectedReceta)
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Receta favorita seleccionada: {selectedReceta.Nombre}");
                await Application.Current.MainPage.DisplayAlert("Receta Favorita Seleccionada", $"Has seleccionado tu favorita: {selectedReceta.Nombre}", "OK");
            }
        }
    }
}
