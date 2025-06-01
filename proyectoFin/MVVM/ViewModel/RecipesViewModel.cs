using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services; // Para tu API
using Domain.Model; // Para tu clase Receta
using System.Linq; // Para el .ToList()
using Refit; // NUEVO: Para ApiException
using System; // Para Exception

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Receta> recipes;

        public IRelayCommand LoadRecipesCommand { get; }
        public IRelayCommand<Receta> SelectRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;

        public RecipesViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            Recipes = new ObservableCollection<Receta>();

            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);
            SelectRecipeCommand = new RelayCommand<Receta>(OnRecipeSelected);

            _ = LoadRecipesAsync();
        }

        private async Task LoadRecipesAsync()
        {
            try
            {
                var response = await _apiService.GetRecetasAsync();
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Recipes.Clear();
                    foreach (var receta in response.Content)
                    {
                        Recipes.Add(receta);
                    }
                }
                else
                {
                    // Manejo de errores de la API
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar las recetas. Código: {response.StatusCode}", "OK");
                }
            }
            catch (ApiException ex) // Ahora ApiException será reconocido
            {
                // Manejo de errores de Refit (HTTP errors, deserialization errors)
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", $"Error al conectar con la API de recetas: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                // Otros errores inesperados
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error inesperado al cargar las recetas: {ex.Message}", "OK");
            }
        }

        private void OnRecipeSelected(Receta selectedReceta)
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Receta seleccionada: {selectedReceta.nombre}");
                Application.Current.MainPage.DisplayAlert("Receta Seleccionada", $"Has seleccionado: {selectedReceta.nombre}", "OK");
                // Aquí podrías navegar a una página de detalles de la receta, pasando el id_receta
                // Ejemplo: await Shell.Current.GoToAsync($"//RecipeDetailPage?id={selectedReceta.id_receta}");
            }
        }
    }
}
