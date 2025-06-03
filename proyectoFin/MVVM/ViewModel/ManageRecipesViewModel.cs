using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // Para NavigationPage y Application.Current.MainPage
using proyectoFin.Services; // Para IBakeOrTakeApi
// using Domain.Model; // ¡ELIMINAR ESTA LÍNEA! Ya no se utiliza la entidad de dominio 'Receta' directamente aquí.
using System; // Para Exception, IServiceProvider
using proyectoFin.MVVM.View; // Para RecipeFormPage
using Microsoft.Maui.Storage; // ¡CORRECTO! Para SecureStorage
using Domain.Model.ApiResponses; // Para RecetaResponse

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ManageRecipesViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<RecetaResponse> recipes; // ¡CORRECTO! Ahora es RecetaResponse

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public IAsyncRelayCommand AddRecipeCommand { get; }
        // Tipo del comando debe ser RecetaResponse
        public IAsyncRelayCommand<RecetaResponse> EditRecipeCommand { get; } // ¡CORRECTO!
        public IAsyncRelayCommand<RecetaResponse> DeleteRecipeCommand { get; } // ¡CORRECTO!
        public IAsyncRelayCommand LoadRecipesCommand { get; }

        public ManageRecipesViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            Recipes = new ObservableCollection<RecetaResponse>(); // ¡CORRECTO!

            AddRecipeCommand = new AsyncRelayCommand(AddRecipe);
            // El método debe aceptar RecetaResponse para que coincida con el comando
            EditRecipeCommand = new AsyncRelayCommand<RecetaResponse>(EditRecipe); // ¡CORRECCIÓN CLAVE AQUÍ!
            DeleteRecipeCommand = new AsyncRelayCommand<RecetaResponse>(DeleteRecipe); // ¡CORRECTO!
            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);

            _ = LoadRecipesAsync();
        }

        private async Task AddRecipe()
        {
            // Lógica para navegación a RecipeFormPage en modo creación (ID 0)
            // (Esta sección ya está bien si quieres mantener la lógica de FlyoutPage o TabbedPage)
            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        await formViewModel.InitializeAsync(0);
                    }
                    await navigationPage.PushAsync(recipeFormPage);
                    flyoutPage.IsPresented = false;
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para añadir recetas.", "OK");
                }
            }
            // Lógica para navegación desde una TabbedPage (si es la actual estructura)
            else if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        await formViewModel.InitializeAsync(0);
                    }
                    await currentTabPageNav.PushAsync(recipeFormPage);
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para añadir recetas.", "OK");
                }
            }
            else
            {
                Console.WriteLine("Error: Contexto de navegación inesperado para AddRecipe.");
                await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
            }
        }

        // ¡CORRECCIÓN CLAVE AQUÍ!: El parámetro debe ser RecetaResponse para coincidir con el comando.
        private async Task EditRecipe(RecetaResponse recipeToEdit)
        {
            if (recipeToEdit == null) return;

            // Lógica para navegación a RecipeFormPage en modo edición (con ID)
            // (Esta sección ya está bien si quieres mantener la lógica de FlyoutPage o TabbedPage)
            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        // Inicializa el ViewModel en modo edición con el ID de la receta
                        await formViewModel.InitializeAsync(recipeToEdit.IdReceta); // ¡CORRECTO!
                    }
                    await navigationPage.PushAsync(recipeFormPage);
                    flyoutPage.IsPresented = false;
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta para edición.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para editar recetas.", "OK");
                }
            }
            // Lógica para navegación desde una TabbedPage (si es la actual estructura)
            else if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        await formViewModel.InitializeAsync(recipeToEdit.IdReceta); // ¡CORRECTO!
                    }
                    await currentTabPageNav.PushAsync(recipeFormPage);
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta para edición.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para editar recetas.", "OK");
                }
            }
            else
            {
                Console.WriteLine("Error: Contexto de navegación inesperado para EditRecipe.");
                await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
            }
        }

        private async Task DeleteRecipe(RecetaResponse recipeToDelete) // ¡CORRECTO!
        {
            if (recipeToDelete == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar Eliminación", $"¿Estás seguro de que quieres eliminar la receta '{recipeToDelete.Nombre}'?", "Sí", "No"); // ¡CORRECTO!
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.DeleteReceta(recipeToDelete.IdReceta); // ¡CORRECTO!

                if (response.IsSuccessStatusCode)
                {
                    Recipes.Remove(recipeToDelete);
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
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idEmpresaActual))
                {
                    ErrorMessage = "No se pudo obtener el ID de la empresa logueada para cargar recetas.";
                    IsBusy = false;
                    return;
                }

                var response = await _apiService.GetRecetasByCompanyAsync(idEmpresaActual); // ¡CORRECTO!

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Recipes.Clear();
                    foreach (var recipeResponse in response.Content) // ¡CORRECTO!
                    {
                        Recipes.Add(recipeResponse);
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