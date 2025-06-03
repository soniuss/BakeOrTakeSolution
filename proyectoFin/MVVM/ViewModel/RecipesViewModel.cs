using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using System.Linq;
using Refit;
using System;
using Domain.Model.ApiResponses; // Para RecetaResponse
using Microsoft.Maui.Storage; // Para SecureStorage

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecipesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RecetaResponse> recipes;

        // ¡CORRECCIÓN CLAVE AQUÍ! Añadir _isBusy y NotifyPropertyChangedFor
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        // ¡CORRECCIÓN CLAVE AQUÍ! Añadir IsNotBusy
        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage; // Asegúrate de que esta propiedad también esté si la usas.


        public IRelayCommand LoadRecipesCommand { get; }
        public IRelayCommand<RecetaResponse> SelectRecipeCommand { get; }

        private readonly IBakeOrTakeApi _apiService;

        public RecipesViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            Recipes = new ObservableCollection<RecetaResponse>();

            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);
            SelectRecipeCommand = new AsyncRelayCommand<RecetaResponse>(OnRecipeSelected); // Cambiado a AsyncRelayCommand

            _ = LoadRecipesAsync(); // Iniciar la carga al construir el ViewModel
        }

        private async Task LoadRecipesAsync()
        {
            // Si ya está ocupado, salir para evitar múltiples cargas
            if (IsBusy) return; // Ahora IsBusy existe

            IsBusy = true; // Ahora IsBusy existe
            ErrorMessage = string.Empty; // Ahora ErrorMessage existe

            // ¡CORRECCIÓN CLAVE AQUÍ! Declarar 'response' fuera del try
            ApiResponse<List<RecetaResponse>> response = null; // Inicializar a null

            try
            {
                // --- LÓGICA PARA ESPERAR EL TOKEN (si la API es protegida) ---
                // Esta lógica es para cuando la API requiere autenticación para GetRecetasAsync.
                // Si tu GetRecetasAsync() en la API REST *no* tiene [Authorize], puedes simplificar esto.
                // Pero si el 401 se debe a una cabecera vacía, esto ayuda.
                string token = await SecureStorage.GetAsync("jwt_token");
                int retryCount = 0;
                const int maxRetries = 3;
                const int retryDelayMs = 500;

                if (string.IsNullOrEmpty(token)) // Solo si el token es nulo al principio
                {
                    while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                    {
                        Console.WriteLine($"DEBUG: Token no encontrado aún. Reintentando en {retryDelayMs}ms... (Intento {retryCount + 1})");
                        await Task.Delay(retryDelayMs);
                        token = await SecureStorage.GetAsync("jwt_token");
                        retryCount++;
                    }
                }

                // Si después de los reintentos el token sigue sin estar disponible,
                // y si esta API requiere autenticación, no podemos continuar.
                // (Aunque GetRecetasAsync() no tenga [Authorize], el 401 sugiere un problema de autenticación).
                if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
                {
                    ErrorMessage = "No se pudo obtener el token de autenticación después de varios intentos. Por favor, intente iniciar sesión de nuevo si el problema persiste.";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                    IsBusy = false;
                    return;
                }
                // --- FIN LÓGICA DE ESPERA ---


                // Realizar la llamada a la API
                response = await _apiService.GetRecetasAsync();

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

        private async Task OnRecipeSelected(RecetaResponse selectedReceta) // Parámetro de tipo RecetaResponse
        {
            if (selectedReceta != null)
            {
                Console.WriteLine($"Receta seleccionada: {selectedReceta.Nombre}");
                await Application.Current.MainPage.DisplayAlert("Receta Seleccionada", $"Has seleccionado: {selectedReceta.Nombre}", "OK");
                // Aquí podrías navegar a RecetaDetallePage pasando selectedReceta.IdReceta
                // Ejemplo: await Application.Current.MainPage.Navigation.PushAsync(_serviceProvider.GetRequiredService<RecetaDetallePage>());
                // Y luego pasar el ID al ViewModel de RecetaDetallePage
            }
        }
    }
}