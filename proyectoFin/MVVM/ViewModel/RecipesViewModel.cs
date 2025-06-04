using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using System.Linq;
using Refit;
using System;
using Domain.Model.ApiResponses; // Para RecetaResponse
using Microsoft.Maui.Storage; // Para SecureStorage
using Microsoft.Extensions.DependencyInjection;
using proyectoFin.MVVM.View; // Para IServiceProvider

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaResponse> recipes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;


        public IRelayCommand LoadRecipesCommand { get; }
        public IAsyncRelayCommand<RecetaResponse> SelectRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider; // Necesario para la navegación

        public RecipesViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider) // Inyectar IServiceProvider
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider; // Asignar el serviceProvider
            Recipes = new ObservableCollection<RecetaResponse>();

            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);
            SelectRecipeCommand = new AsyncRelayCommand<RecetaResponse>(OnRecipeSelected);

            _ = LoadRecipesAsync(); // Iniciar la carga al construir el ViewModel
        }

        private async Task LoadRecipesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            ApiResponse<List<RecetaResponse>> response = null;

            try
            {
                string token = await SecureStorage.GetAsync("jwt_token");
                int retryCount = 0;
                const int maxRetries = 3;
                const int retryDelayMs = 500;

                if (string.IsNullOrEmpty(token))
                {
                    while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                    {
                        Console.WriteLine($"DEBUG: Token no encontrado aún. Reintentando en {retryDelayMs}ms... (Intento {retryCount + 1})");
                        await Task.Delay(retryDelayMs);
                        token = await SecureStorage.GetAsync("jwt_token");
                        retryCount++;
                    }
                }

                if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
                {
                    ErrorMessage = "No se pudo obtener el token de autenticación después de varios intentos. Por favor, intente iniciar sesión de nuevo si el problema persiste.";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                    IsBusy = false;
                    return;
                }

                response = await _apiService.GetRecetasAsync();

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Recipes.Clear();
                    foreach (var recetaResponse in response.Content)
                    {
                        Recipes.Add(recetaResponse);
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar las recetas. Código: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar las recetas: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnRecipeSelected(RecetaResponse selectedReceta)
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Receta seleccionada: {selectedReceta.Nombre}");
                // await Application.Current.MainPage.DisplayAlert("Receta Seleccionada", $"Has seleccionado: {selectedReceta.Nombre}", "OK"); // Eliminar esta línea

                // ¡CORRECCIÓN CLAVE AQUÍ! Implementar la navegación a RecetaDetallePage
                if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
                {
                    var recetaDetallePage = _serviceProvider.GetService<RecetaDetallePage>();
                    if (recetaDetallePage != null)
                    {
                        if (recetaDetallePage.BindingContext is RecetaDetalleViewModel detalleViewModel)
                        {
                            detalleViewModel.RecetaId = selectedReceta.IdReceta; // Pasar el ID de la receta
                            await detalleViewModel.LoadRecetaCommand.ExecuteAsync(null); // Cargar los detalles
                        }
                        await currentTabPageNav.PushAsync(recetaDetallePage); // Navegar
                    }
                    else
                    {
                        Console.WriteLine("Error: RecetaDetallePage no pudo ser resuelta.");
                        await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página de detalles de la receta.", "OK");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Contexto de navegación inesperado para OnRecipeSelected.");
                    await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
                }
            }
        }
    }
}
