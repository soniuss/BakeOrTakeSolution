using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using Domain.Model.ApiResponses; // Para RecetaResponse
using Microsoft.Maui.Storage; // Para SecureStorage
using System;
using proyectoFin.MVVM.View; // Para RecipeFormPage, RecetaDetallePage

namespace proyectoFin.MVVM.ViewModel
{
    public partial class MyRecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaResponse> myRecipes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

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

            LoadMyRecipesCommand = new AsyncRelayCommand(LoadMyRecipesAsync);
            SelectMyRecipeCommand = new AsyncRelayCommand<RecetaResponse>(OnMyRecipeSelected);
            AddNewRecipeCommand = new AsyncRelayCommand(AddNewRecipe);
            EditRecipeCommand = new AsyncRelayCommand<RecetaResponse>(EditRecipe);
            DeleteRecipeCommand = new AsyncRelayCommand<RecetaResponse>(DeleteRecipe);

            _ = LoadMyRecipesAsync();
        }

        private async Task LoadMyRecipesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            // ¡NUEVA LÓGICA PARA ESPERAR EL TOKEN!
            string token = await SecureStorage.GetAsync("jwt_token");
            int retryCount = 0;
            const int maxRetries = 3;
            const int retryDelayMs = 500;

            if (string.IsNullOrEmpty(token))
            {
                while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                {
                    Console.WriteLine($"DEBUG: Token no encontrado aún en MyRecipesViewModel. Reintentando... (Intento {retryCount + 1})");
                    await Task.Delay(retryDelayMs);
                    token = await SecureStorage.GetAsync("jwt_token");
                    retryCount++;
                }
            }

            if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
            {
                ErrorMessage = "No se pudo obtener el token de autenticación para cargar tus recetas. Por favor, intente iniciar sesión de nuevo.";
                await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                IsBusy = false;
                return;
            }
            // --- FIN LÓGICA DE ESPERA ---

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado.";
                    return;
                }

                var response = await _apiService.GetRecetasByCreatorAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MyRecipes.Clear();
                    foreach (var recetaResponse in response.Content)
                    {
                        MyRecipes.Add(recetaResponse);
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar tus recetas. Código: {response.StatusCode}. Detalles: {errorContent}";
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
                ErrorMessage = $"Ocurrió un error inesperado al cargar tus recetas: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnMyRecipeSelected(RecetaResponse selectedReceta)
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Mi receta seleccionada: {selectedReceta.Nombre}");
                await NavigateToRecipeDetailPage(selectedReceta.IdReceta);
            }
        }

        private async Task AddNewRecipe()
        {
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
                    MyRecipes.Remove(recipeToDelete);
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

        private async Task NavigateToRecipeDetailPage(int recipeId)
        {
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
