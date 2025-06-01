using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain.Model; // ¡IMPORTANTE! Para tu clase Receta
using proyectoFin.Services; // Para tu API (si tienes un endpoint para favoritos)
using System.Linq;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class FavoritesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Receta> favoriteRecipes; // ¡Ahora es de tipo Receta!

        public IRelayCommand LoadFavoriteRecipesCommand { get; }
        public IRelayCommand<Receta> SelectFavoriteRecipeCommand { get; } // ¡Ahora espera una Receta!

        private readonly IBakeOrTakeApi _apiService; // Si necesitas un endpoint para favoritos

        public FavoritesViewModel(IBakeOrTakeApi apiService) // Asegúrate de inyectar el servicio API
        {
            _apiService = apiService;
            FavoriteRecipes = new ObservableCollection<Receta>();

            LoadFavoriteRecipesCommand = new AsyncRelayCommand(LoadFavoriteRecipesAsync);
            SelectFavoriteRecipeCommand = new RelayCommand<Receta>(OnFavoriteRecipeSelected);

            _ = LoadFavoriteRecipesAsync();
        }

        private async Task LoadFavoriteRecipesAsync()
        {
            // Lógica para cargar las recetas favoritas del usuario
            // Asume que tu API tiene un endpoint para obtener los favoritos de un cliente.
            // Esto podría requerir un nuevo método en IBakeOrTakeApi, por ejemplo:
            // Task<ApiResponse<List<Favorito>>> GetFavoritosByClienteAsync(int idCliente);
            // Si ese es el caso, tendrías que mapear los Favoritos a Recetas.

            // Ejemplo asumiendo un endpoint que devuelve directamente las Recetas favoritas:
            // try
            // {
            //     var response = await _apiService.GetFavoriteRecipesForClientAsync(id_cliente_actual);
            //     if (response.IsSuccessStatusCode && response.Content != null)
            //     {
            //         FavoriteRecipes.Clear();
            //         foreach (var receta in response.Content)
            //         {
            //             FavoriteRecipes.Add(receta);
            //         }
            //     }
            // }
            // catch (Exception ex)
            // {
            //     await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar las recetas favoritas: {ex.Message}", "OK");
            // }

            // Datos de prueba (directamente Recetas):
            if (FavoriteRecipes.Count == 0)
            {
                FavoriteRecipes.Add(new Receta { nombre = "Receta tarta de chocolate", descripcion = "Tarta de chocolate", id_receta = 201, id_cliente_creador = 5 });
                FavoriteRecipes.Add(new Receta { nombre = "Smoothie Energético", descripcion = "Batido de frutas y verduras", id_receta = 202, id_cliente_creador = 6 });
            }
        }

        private void OnFavoriteRecipeSelected(Receta selectedReceta) // Recibe una Receta
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Receta favorita seleccionada: {selectedReceta.nombre}");
                Application.Current.MainPage.DisplayAlert("Receta Favorita Seleccionada", $"Has seleccionado tu favorita: {selectedReceta.nombre}", "OK");
                // Navegar a los detalles de esta receta
            }
        }
    }
}