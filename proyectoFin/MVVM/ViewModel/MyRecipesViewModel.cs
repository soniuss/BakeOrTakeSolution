using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using proyectoFin.Services;
using Domain.Model.ApiResponses;
using proyectoFin.MVVM.View;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class MyRecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaResponse> myRecipes;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        public bool IsNotBusy => !IsBusy;

        // ¡CORRECCIÓN CLAVE AQUÍ! Debe ser IAsyncRelayCommand
        public IAsyncRelayCommand LoadMyRecipesCommand { get; }

        public IAsyncRelayCommand<RecetaResponse> SelectMyRecipeCommand { get; }
        public IAsyncRelayCommand AddNewRecipeCommand { get; }
        public IAsyncRelayCommand<RecetaResponse> EditRecipeCommand { get; }
        public IAsyncRelayCommand<RecetaResponse> DeleteRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        public MyRecipesViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            MyRecipes = new ObservableCollection<RecetaResponse>();

            // La inicialización ya era correcta como AsyncRelayCommand
            LoadMyRecipesCommand = new AsyncRelayCommand(LoadMyRecipesAsync);
            SelectMyRecipeCommand = new AsyncRelayCommand<RecetaResponse>(OnMyRecipeSelected);
            AddNewRecipeCommand = new AsyncRelayCommand(AddNewRecipe);
            EditRecipeCommand = new AsyncRelayCommand<RecetaResponse>(EditRecipe);
            DeleteRecipeCommand = new AsyncRelayCommand<RecetaResponse>(DeleteRecipe);
        }

        private async Task LoadMyRecipesAsync()
        {
            Console.WriteLine("DEBUG: MyRecipesViewModel - LoadMyRecipesAsync iniciado.");

            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            string token = await SecureStorage.GetAsync("jwt_token");
            int retryCount = 0;
            const int maxRetries = 3;
            const int retryDelayMs = 500;

            if (string.IsNullOrEmpty(token))
            {
                while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                {
                    Console.WriteLine($"DEBUG: MyRecipesViewModel - Token no encontrado aún. Reintentando... (Intento {retryCount + 1})");
                    await Task.Delay(retryDelayMs);
                    token = await SecureStorage.GetAsync("jwt_token");
                    retryCount++;
                }
            }

            if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
            {
                ErrorMessage = "No se pudo obtener el token de autenticación para cargar tus recetas. Por favor, intente iniciar sesión de nuevo si el problema persiste.";
                await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                IsBusy = false;
                return;
            }

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado.";
                    return;
                }
                Console.WriteLine($"DEBUG: MyRecipesViewModel - idClienteActual para GetRecetasByCreator: {idClienteActual}");

                var response = await _apiService.GetRecetasByCreatorAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MyRecipes.Clear();
                    Console.WriteLine($"DEBUG: MyRecipesViewModel - Recetas recibidas de la API para el creador {idClienteActual}: {response.Content.Count}");
                    foreach (var recetaResponse in response.Content)
                    {
                        Console.WriteLine($"DEBUG: MyRecipesViewModel - Receta añadida: {recetaResponse.Nombre} (ID: {recetaResponse.IdReceta})");
                        MyRecipes.Add(recetaResponse);
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar tus recetas. Código: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                    Console.WriteLine($"ERROR: MyRecipesViewModel - {ErrorMessage}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
                Console.WriteLine($"ERROR: MyRecipesViewModel - {ErrorMessage}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar tus recetas: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                Console.WriteLine($"ERROR: MyRecipesViewModel - {ErrorMessage}");
            }
            finally
            {
                IsBusy = false;
                Console.WriteLine("DEBUG: MyRecipesViewModel - LoadMyRecipesAsync finalizado.");
            }
        }

        private async Task OnMyRecipeSelected(RecetaResponse selectedReceta)
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"DEBUG: MyRecipesViewModel - Receta seleccionada para detalle: {selectedReceta.Nombre}");
                await NavigateToRecipeDetailPage(selectedReceta.IdReceta);
            }
        }

        private async Task AddNewRecipe()
        {
            Console.WriteLine("DEBUG: MyRecipesViewModel - AddNewRecipe iniciado.");
            if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
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
                Console.WriteLine("Error: Contexto de navegación inesperado para AddNewRecipe.");
                await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
            }
        }

        private async Task EditRecipe(RecetaResponse recipeToEdit)
        {
            Console.WriteLine($"DEBUG: MyRecipesViewModel - EditRecipe iniciado para ID: {recipeToEdit.IdReceta}");
            if (recipeToEdit == null) return;

            if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        await formViewModel.InitializeAsync(recipeToEdit.IdReceta);
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

        private async Task DeleteRecipe(RecetaResponse recipeToDelete)
        {
            Console.WriteLine($"DEBUG: MyRecipesViewModel - DeleteRecipe iniciado para ID: {recipeToDelete.IdReceta}");
            if (recipeToDelete == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar Eliminación", $"¿Estás seguro de que quieres eliminar la receta '{recipeToDelete.Nombre}'?", "Sí", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.DeleteReceta(recipeToDelete.IdReceta);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Receta eliminada exitosamente.", "OK");
                    MyRecipes.Remove(recipeToDelete);
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al eliminar receta: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                    Console.WriteLine($"ERROR: MyRecipesViewModel - {ErrorMessage}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
                Console.WriteLine($"ERROR: MyRecipesViewModel - {ErrorMessage}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al eliminar la receta: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                Console.WriteLine($"ERROR: MyRecipesViewModel - {ErrorMessage}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToRecipeDetailPage(int recipeId)
        {
            Console.WriteLine($"DEBUG: MyRecipesViewModel - Navegando a detalle de receta ID: {recipeId}");
            if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                var recetaDetallePage = _serviceProvider.GetService<RecetaDetallePage>();
                if (recetaDetallePage != null)
                {
                    if (recetaDetallePage.BindingContext is RecetaDetalleViewModel detalleViewModel)
                    {
                        detalleViewModel.RecetaId = recipeId;
                        await detalleViewModel.LoadRecetaCommand.ExecuteAsync(null);
                    }
                    await currentTabPageNav.PushAsync(recetaDetallePage);
                }
                else
                {
                    Console.WriteLine("Error: RecetaDetallePage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página de detalles de la receta.", "OK");
                }
            }
            else
            {
                Console.WriteLine("Error: Contexto de navegación inesperado para NavigateToRecipeDetailPage.");
                await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
            }
        }
    }
}
