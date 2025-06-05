using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using Refit;
using System;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using proyectoFin.MVVM.View; // Para RecetaDetallePage
using Domain.Model.ApiResponses; // Para RecetaResponse

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaItemViewModel> recipes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public IRelayCommand LoadRecipesCommand { get; }
        public IAsyncRelayCommand<RecetaItemViewModel> SelectRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        // ¡CORRECCIÓN CLAVE AQUÍ! SelectedItem se declara con [ObservableProperty]
        // y el setter automático se encargará de OnSelectedItemChanged.
        [ObservableProperty]
        private RecetaItemViewModel _selectedItem;

        // ¡ELIMINAR LA IMPLEMENTACIÓN MANUAL DE OnSelectedItemChanged!
        // El generador de código ya crea un método parcial vacío o una implementación básica.
        // partial void OnSelectedItemChanged(RecetaItemViewModel value)
        // {
        //    foreach (var item in Recipes)
        //        item.IsSelected = false;
        //    if (value != null)
        //        value.IsSelected = true;
        // }


        public RecipesViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            Recipes = new ObservableCollection<RecetaItemViewModel>();

            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);
            SelectRecipeCommand = new AsyncRelayCommand<RecetaItemViewModel>(OnRecipeSelected);

            _ = LoadRecipesAsync();
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
                        // Envolver cada RecetaResponse en RecetaItemViewModel
                        Recipes.Add(new RecetaItemViewModel(recetaResponse));
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

        private async Task OnRecipeSelected(RecetaItemViewModel selectedItem)
        {
            if (selectedItem?.Receta == null)
                return;

            var selectedReceta = selectedItem.Receta;

            // Lógica de selección visual para todos los ítems
            foreach (var item in Recipes)
            {
                item.IsSelected = (item == selectedItem); // Marcar solo el ítem actual como seleccionado
            }

            // Navegación a RecetaDetallePage
            if (Application.Current.MainPage is NavigationPage mainNavPage &&
                mainNavPage.CurrentPage is TabbedPage tabbedPage &&
                tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                var recetaDetallePage = _serviceProvider.GetService<RecetaDetallePage>();
                if (recetaDetallePage?.BindingContext is RecetaDetalleViewModel detalleViewModel)
                {
                    detalleViewModel.RecetaId = selectedReceta.IdReceta;
                    await detalleViewModel.LoadRecetaCommand.ExecuteAsync(null);
                }

                await currentTabPageNav.PushAsync(recetaDetallePage);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
            }
        }
    }
}
