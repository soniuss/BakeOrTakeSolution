using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // Para NavigationPage y Application.Current.MainPage
using proyectoFin.Services; // Para IBakeOrTakeApi
using Domain.Model; // Para la entidad Receta
using System; // Para Exception, IServiceProvider
using proyectoFin.MVVM.View; // Para RecipeFormPage
using Microsoft.Maui.Storage; // Para SecureStorage

namespace proyectoFin.MVVM.ViewModel
{
    // Asegúrate de que esta clase es partial si usas [ObservableProperty] o [RelayCommand]
    public partial class ManageRecipesViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService; // Renombrado de _api a _apiService para consistencia
        private readonly IServiceProvider _serviceProvider; // Inyectar IServiceProvider

        [ObservableProperty]
        private ObservableCollection<Receta> recipes; // Colección de recetas de la empresa

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))] // Notifica cuando IsBusy cambia
        private bool _isBusy; // Para el indicador de actividad

        public bool IsNotBusy => !IsBusy; // Propiedad calculada

        [ObservableProperty]
        private string _errorMessage; // Para mostrar mensajes de error a la UI

        // Comandos
        public IAsyncRelayCommand AddRecipeCommand { get; }
        public IAsyncRelayCommand<Receta> EditRecipeCommand { get; }
        public IAsyncRelayCommand<Receta> DeleteRecipeCommand { get; } // Comando para eliminar recetas
        public IAsyncRelayCommand LoadRecipesCommand { get; } // Para cargar las recetas al iniciar

        public ManageRecipesViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider) // Recibir IServiceProvider
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider; // Asignar el servicio
            Recipes = new ObservableCollection<Receta>();

            AddRecipeCommand = new AsyncRelayCommand(AddRecipe);
            EditRecipeCommand = new AsyncRelayCommand<Receta>(EditRecipe);
            DeleteRecipeCommand = new AsyncRelayCommand<Receta>(DeleteRecipe); // Inicializar comando de eliminación
            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);

            // Cargar recetas al iniciar el ViewModel
            _ = LoadRecipesAsync();
        }

        private async Task AddRecipe()
        {
            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        // Inicializa el ViewModel en modo creación (ID 0)
                        await formViewModel.InitializeAsync(0);
                    }
                    await navigationPage.PushAsync(recipeFormPage);
                    flyoutPage.IsPresented = false; // Cierra el menú Flyout
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para añadir recetas.", "OK");
                }
            }
        }

        private async Task EditRecipe(Receta recipeToEdit)
        {
            if (recipeToEdit == null) return;

            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        // Inicializa el ViewModel en modo edición con el ID de la receta
                        await formViewModel.InitializeAsync(recipeToEdit.id_receta);
                    }
                    await navigationPage.PushAsync(recipeFormPage);
                    flyoutPage.IsPresented = false; // Cierra el menú Flyout
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta para edición.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para editar recetas.", "OK");
                }
            }
        }

        private async Task DeleteRecipe(Receta recipeToDelete)
        {
            if (recipeToDelete == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar Eliminación", $"¿Estás seguro de que quieres eliminar la receta '{recipeToDelete.nombre}'?", "Sí", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // La API REST (RecetasController) se encargará de verificar los permisos
                var response = await _apiService.DeleteReceta(recipeToDelete.id_receta);

                if (response.IsSuccessStatusCode)
                {
                    Recipes.Remove(recipeToDelete); // Eliminar de la colección local
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Receta eliminada exitosamente.", "OK");
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al eliminar receta: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al eliminar la receta: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadRecipesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Obtener el ID de la EMPRESA logueada desde SecureStorage
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idEmpresaActual))
                {
                    ErrorMessage = "No se pudo obtener el ID de la empresa logueada para cargar recetas.";
                    IsBusy = false;
                    return;
                }

                // Llamada al método GetRecetasByCompanyAsync en IBakeOrTakeApi
                var response = await _apiService.GetRecetasByCompanyAsync(idEmpresaActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Recipes.Clear();
                    foreach (var recipe in response.Content)
                    {
                        Recipes.Add(recipe);
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar las recetas de la empresa. Código: {response.StatusCode}. Detalles: {errorContent}";
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
                ErrorMessage = $"Ocurrió un error inesperado al cargar las recetas de la empresa: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
