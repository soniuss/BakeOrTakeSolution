using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using Domain.Model;
using System.Linq;
using Microsoft.Maui.Storage;
using System;
using proyectoFin.MVVM.View;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class MyRecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Receta> myRecipes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))] // <-- Asegúrate de que este atributo está en _isBusy
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy; // <-- Asegúrate de que esta propiedad está definida

        [ObservableProperty]
        private string _errorMessage;

        public IRelayCommand LoadMyRecipesCommand { get; }
        public IRelayCommand<Receta> SelectMyRecipeCommand { get; }
        public IRelayCommand AddNewRecipeCommand { get; }
        public IRelayCommand<Receta> EditRecipeCommand { get; }
        public IRelayCommand<Receta> DeleteRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        public MyRecipesViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            MyRecipes = new ObservableCollection<Receta>();

            LoadMyRecipesCommand = new AsyncRelayCommand(LoadMyRecipesAsync);
            SelectMyRecipeCommand = new RelayCommand<Receta>(OnMyRecipeSelected);
            AddNewRecipeCommand = new AsyncRelayCommand(AddNewRecipe);
            EditRecipeCommand = new AsyncRelayCommand<Receta>(EditRecipe);
            DeleteRecipeCommand = new AsyncRelayCommand<Receta>(DeleteRecipe);

            _ = LoadMyRecipesAsync();
        }

        private async Task LoadMyRecipesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

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
                    foreach (var receta in response.Content)
                    {
                        MyRecipes.Add(receta);
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

        private async void OnMyRecipeSelected(Receta selectedReceta)
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Mi receta seleccionada: {selectedReceta.nombre}");
                await NavigateToRecipeDetailPage(selectedReceta.id_receta);
            }
        }

        private async Task AddNewRecipe()
        {
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
                        await formViewModel.InitializeAsync(recipeToEdit.id_receta);
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
                var response = await _apiService.DeleteReceta(recipeToDelete.id_receta);

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
            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var recetaDetallePage = _serviceProvider.GetService<RecetaDetallePage>();
                if (recetaDetallePage != null)
                {
                    if (recetaDetallePage.BindingContext is RecetaDetalleViewModel detalleViewModel)
                    {
                        detalleViewModel.RecetaId = recipeId;
                        // CORRECCIÓN: Llamar al comando generado por [RelayCommand]
                        await detalleViewModel.LoadRecetaCommand.ExecuteAsync(null);
                    }
                    await navigationPage.PushAsync(recetaDetallePage);
                    flyoutPage.IsPresented = false;
                }
                else
                {
                    Console.WriteLine("Error: RecetaDetallePage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página de detalles de la receta.", "OK");
                }
            }
        }
    }
}
