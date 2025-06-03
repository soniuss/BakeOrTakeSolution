using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // Para tu API
using proyectoFin.Services;
using System.Linq; // Para el .ToList()
using Refit; // Para ApiException
using System;
using Domain.Model.ApiResponses; // ¡CORRECTO! Para RecetaResponse

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaResponse> recipes;

       
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;
        public IRelayCommand<RecetaResponse> SelectRecipeCommand { get; }
        public IRelayCommand LoadRecipesCommand { get; } // Este no cambia, es un comando sin parámetros

        private readonly IBakeOrTakeApi _apiService;

        public RecipesViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            Recipes = new ObservableCollection<RecetaResponse>(); // ¡CORRECCIÓN CLAVE AQUÍ! Inicializar con RecetaResponse

            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);
            SelectRecipeCommand = new RelayCommand<RecetaResponse>(OnRecipeSelected); // ¡CORRECCIÓN CLAVE AQUÍ! Asignar con RecetaResponse

            _ = LoadRecipesAsync(); // Iniciar la carga al construir el ViewModel
        }

        private async Task LoadRecipesAsync()
        {
            // Si ya está ocupado, salir para evitar múltiples cargas
            if (IsBusy) return;

            IsBusy = true; // Activar indicador de actividad
            ErrorMessage = string.Empty; // Limpiar mensaje de error

            try
            {
                var response = await _apiService.GetRecetasAsync();
                // --- NUEVA LÓGICA PARA ESPERAR EL TOKEN (si la API es protegida) ---
                string token = await SecureStorage.GetAsync("jwt_token");
                int retryCount = 0;
                const int maxRetries = 3; // Número máximo de reintentos
                const int retryDelayMs = 500; // Retraso entre reintentos en milisegundos (0.5 segundos)

                // Esperar a que el token esté disponible, con reintentos
                // Si el endpoint GetRecetasAsync() *no* está protegido, este bucle no es estrictamente necesario,
                // pero si el 401 se debe a una cabecera vacía, puede ayudar.
                while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                {
                    Console.WriteLine($"DEBUG: Token no encontrado aún. Reintentando en {retryDelayMs}ms... (Intento {retryCount + 1})");
                    await Task.Delay(retryDelayMs); // Esperar antes de reintentar
                    token = await SecureStorage.GetAsync("jwt_token"); // Intentar obtener el token de nuevo
                    retryCount++;
                }

                // Si después de los reintentos el token sigue sin estar disponible,
                // y si esta API requiere autenticación, no podemos continuar.
                // (Aunque GetRecetasAsync() no tenga [Authorize], el 401 sugiere un problema de autenticación).
                if (string.IsNullOrEmpty(token) && response == null) // Solo si no se ha hecho ninguna llamada exitosa
                {
                    ErrorMessage = "No se pudo obtener el token de autenticación. Por favor, intente iniciar sesión de nuevo si el problema persiste.";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                    IsBusy = false;
                    return; // Salir si el token no está disponible y la API es protegida
                }
                // --- FIN LÓGICA DE ESPERA ---



                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Recipes.Clear(); // Limpiar la colección antes de añadir nuevas recetas
                    foreach (var recetaResponse in response.Content)
                    {
                        Recipes.Add(recetaResponse);
                    }
                }
                else
                {
                    // Manejo de errores de la API (códigos 4xx, 5xx)
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar las recetas. Código: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex) // Captura errores HTTP de Refit
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex) // Captura cualquier otra excepción inesperada
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar las recetas: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false; // Desactivar indicador de actividad
            }
        }

        private void OnRecipeSelected(RecetaResponse selectedReceta) // ¡CORRECCIÓN CLAVE AQUÍ! Parámetro de tipo RecetaResponse
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Receta seleccionada: {selectedReceta.Nombre}"); // ¡CORRECCIÓN CLAVE AQUÍ! Usar .Nombre (PascalCase)
                Application.Current.MainPage.DisplayAlert("Receta Seleccionada", $"Has seleccionado: {selectedReceta.Nombre}", "OK"); // ¡CORRECCIÓN CLAVE AQUÍ! Usar .Nombre (PascalCase)
                // Aquí podrías navegar a una página de detalles de la receta, pasando el id_receta
                // Ejemplo: await Application.Current.MainPage.Navigation.PushAsync(new RecetaDetallePage(selectedReceta.IdReceta));
            }
        }
    }
}