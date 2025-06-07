using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model; // Necesario para la entidad Receta que se ENVÍA a la API
using Domain.Model.ApiResponses; // Necesario para RecetaResponse que se RECIBE de la API
using proyectoFin.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Refit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection; // Necesario para IServiceProvider

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipeFormViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private int _recetaId;

        [ObservableProperty]
        private string _nombre;

        [ObservableProperty]
        private string _descripcion;

        [ObservableProperty]
        private string _ingredientes;

        [ObservableProperty]
        private string _pasos;

        [ObservableProperty]
        private string _imagenUrl;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public bool IsNewRecipe => RecetaId == 0;
        public string SaveButtonText => "Guardar";

        public IAsyncRelayCommand SaveRecipeCommand { get; }
        public IAsyncRelayCommand LoadRecipeCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public RecipeFormViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            SaveRecipeCommand = new AsyncRelayCommand(SaveRecipe);
            LoadRecipeCommand = new AsyncRelayCommand(LoadRecipe);
            CancelCommand = new RelayCommand(async () => await PerformCancel());
        }

        public async Task InitializeAsync(int recetaId = 0)
        {
            RecetaId = recetaId;
            OnPropertyChanged(nameof(IsNewRecipe));
            OnPropertyChanged(nameof(SaveButtonText));

            if (!IsNewRecipe)
            {
                await LoadRecipeCommand.ExecuteAsync(null);
            }
            else
            {
                Nombre = string.Empty;
                Descripcion = string.Empty;
                Ingredientes = string.Empty;
                Pasos = string.Empty;
                ImagenUrl = string.Empty;
            }
        }

        private async Task LoadRecipe()
        {
            if (IsBusy || RecetaId == 0) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.GetRecetaByIdAsync(RecetaId);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var recetaResponse = response.Content;
                    Nombre = recetaResponse.Nombre;
                    Descripcion = recetaResponse.Descripcion;
                    Ingredientes = recetaResponse.Ingredientes;
                    Pasos = recetaResponse.Pasos;
                    ImagenUrl = recetaResponse.ImagenUrl;
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al cargar receta: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al cargar receta: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al cargar receta: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar la receta: {ex.Message}";
                Console.WriteLine($"General Exception en LoadRecipe: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveRecipe()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Descripcion) ||
                string.IsNullOrWhiteSpace(Ingredientes) || string.IsNullOrWhiteSpace(Pasos))
            {
                ErrorMessage = "Por favor, completa todos los campos de la receta.";
                IsBusy = false;
                return;
            }

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteCreador))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado.";
                    IsBusy = false;
                    return;
                }

                var recetaToSave = new Domain.Model.Receta
                {
                    nombre = Nombre,
                    descripcion = Descripcion,
                    ingredientes = Ingredientes,
                    pasos = Pasos,
                    imagenUrl = ImagenUrl,
                    id_cliente_creador = idClienteCreador
                };

                ApiResponse<RecetaResponse> response;

                if (IsNewRecipe)
                {
                    response = await _apiService.CreateRecetaAsync(recetaToSave);
                }
                else
                {
                    recetaToSave.id_receta = RecetaId;
                    response = await _apiService.UpdateRecetaAsync(RecetaId, recetaToSave);
                }

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", IsNewRecipe ? "Receta creada exitosamente." : "Receta actualizada exitosamente.", "OK");

                    // Lógica de navegación de regreso
                    if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
                    {
                        Console.WriteLine("DEBUG: Navegando de regreso desde TabbedPage context (SaveRecipe).");
                        await currentTabPageNav.PopAsync();
                    }
                    else if (Application.Current.MainPage is NavigationPage directNavPage)
                    {
                        Console.WriteLine("DEBUG: Navegando de regreso desde NavigationPage directa (SaveRecipe).");
                        await directNavPage.PopAsync();
                    }
                    else
                    {
                        Console.WriteLine("ADVERTENCIA: Contexto de navegación inesperado para PopAsync en SaveRecipe. No se pudo volver.");
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al {(IsNewRecipe ? "crear" : "actualizar")} receta: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al {(IsNewRecipe ? "crear" : "actualizar")} receta: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al guardar receta: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al guardar la receta: {ex.Message}";
                Console.WriteLine($"General Exception en SaveRecipe: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PerformCancel()
        {
            Console.WriteLine("DEBUG: PerformCancel() llamado."); // ¡NUEVO!
            if (IsBusy)
            {
                Console.WriteLine("DEBUG: PerformCancel() - ViewModel ocupado, no se puede cancelar."); // ¡NUEVO!
                return;
            }

            // Lógica para volver a la página anterior sin guardar
            if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                Console.WriteLine("DEBUG: Cancelando y volviendo desde TabbedPage context."); // ¡NUEVO!
                await currentTabPageNav.PopAsync(); // Vuelve a la página anterior
            }
            else if (Application.Current.MainPage is NavigationPage directNavPage)
            {
                Console.WriteLine("DEBUG: Cancelando y volviendo desde NavigationPage directa."); // ¡NUEVO!
                await directNavPage.PopAsync();
            }
            else
            {
                Console.WriteLine("ADVERTENCIA: Contexto de navegación inesperado para cancelar. No se pudo volver."); // ¡NUEVO!
            }
        }
    }
}
