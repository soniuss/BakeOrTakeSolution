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
using System.Collections.Generic; // Aunque no directamente usado por estas líneas, si lo necesita tu InitializeAsync

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipeFormViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

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
        public string SaveButtonText => IsNewRecipe ? "Crear Receta" : "Guardar Cambios";

        public IAsyncRelayCommand SaveRecipeCommand { get; }
        public IAsyncRelayCommand LoadRecipeCommand { get; }

        public RecipeFormViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            SaveRecipeCommand = new AsyncRelayCommand(SaveRecipe);
            LoadRecipeCommand = new AsyncRelayCommand(LoadRecipe);
        }

        public async Task InitializeAsync(int recetaId = 0)
        {
            RecetaId = recetaId;
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
                    // ¡CORRECCIÓN CLAVE AQUÍ! Usar PascalCase para las propiedades de RecetaResponse
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

                // Aquí 'recetaToSave' sigue siendo del tipo de dominio 'Receta'
                // porque es lo que tu API espera en el cuerpo de la petición POST/PUT.
                var recetaToSave = new Receta
                {
                    nombre = Nombre,        // Propiedades en camelCase/snake_case del modelo de dominio
                    descripcion = Descripcion,
                    ingredientes = Ingredientes,
                    pasos = Pasos,
                    imagenUrl = ImagenUrl,
                    id_cliente_creador = idClienteCreador
                };

                // ¡CORRECCIÓN CLAVE AQUÍ! La variable 'response' debe ser ApiResponse<RecetaResponse>
                ApiResponse<RecetaResponse> response;

                if (IsNewRecipe)
                {
                    response = await _apiService.CreateRecetaAsync(recetaToSave);
                }
                else
                {
                    recetaToSave.id_receta = RecetaId; // Asigna el ID a la entidad de dominio
                    response = await _apiService.UpdateRecetaAsync(RecetaId, recetaToSave);
                }

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", IsNewRecipe ? "Receta creada exitosamente." : "Receta actualizada exitosamente.", "OK");

                    // Lógica de navegación de regreso a la página anterior (Mis Recetas)
                    if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
                    {
                        await currentTabPageNav.PopAsync(); // Vuelve a la página anterior
                    }
                    // Eliminar la lógica FlyoutPage si ya no se usa (la he dejado como comentario)
                    else if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
                    {
                        await navigationPage.PopAsync();
                    }
                    else
                    {
                        Console.WriteLine("Advertencia: Contexto de navegación inesperado para volver. Simplemente cerrando.");
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
    }
}