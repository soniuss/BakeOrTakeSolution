using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain.Model; // Para la entidad Cliente
using proyectoFin.Services; // Para IBakeOrTakeApi
using Microsoft.Maui.Storage; // Para SecureStorage
using System; // Para Exception, IServiceProvider
using Refit;
using proyectoFin.MVVM.View; // Para ApiException

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider; // Necesario para navegar al logout

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userEmail;

        [ObservableProperty]
        private string userUbicacion;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public IRelayCommand EditProfileCommand { get; }
        public IRelayCommand LogoutCommand { get; }
        public IAsyncRelayCommand LoadUserProfileCommand { get; }

        public ProfileViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            EditProfileCommand = new AsyncRelayCommand(EditProfileAsync);
            LogoutCommand = new RelayCommand(async () => await PerformLogout());
            LoadUserProfileCommand = new AsyncRelayCommand(LoadUserProfile);

            _ = LoadUserProfile(); // Iniciar la carga al construir el ViewModel
        }

        private async Task LoadUserProfile()
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
                    IsBusy = false;
                    return;
                }

                // --- LÓGICA DE ESPERA/REINTENTO PARA EL TOKEN ---
                string token = await SecureStorage.GetAsync("jwt_token");
                int retryCount = 0;
                const int maxRetries = 3;
                const int retryDelayMs = 500; // Retraso de 0.5 segundos

                if (string.IsNullOrEmpty(token)) // Solo si el token es nulo al principio
                {
                    while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                    {
                        Console.WriteLine($"DEBUG: Token no encontrado aún en ProfileViewModel. Reintentando... (Intento {retryCount + 1})");
                        await Task.Delay(retryDelayMs); // Esperar antes de reintentar
                        token = await SecureStorage.GetAsync("jwt_token"); // Intentar obtener el token de nuevo
                        retryCount++;
                    }
                }

                // Si después de los reintentos el token sigue sin estar disponible,
                // y esta API requiere autenticación, no podemos continuar.
                if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
                {
                    ErrorMessage = "No se pudo obtener el token de autenticación para cargar el perfil. Por favor, intente iniciar sesión de nuevo si el problema persiste.";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                    IsBusy = false;
                    return;
                }
                // --- FIN LÓGICA DE ESPERA ---


                var response = await _apiService.GetClienteByIdAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var cliente = response.Content;
                    UserName = cliente.nombre;
                    UserEmail = cliente.email;
                    UserUbicacion = cliente.ubicacion;
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al cargar perfil: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al cargar perfil de cliente: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al cargar perfil: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar el perfil: {ex.Message}";
                Console.WriteLine($"General Exception en LoadUserProfile: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task EditProfileAsync()
        {
            await Application.Current.MainPage.DisplayAlert("Editar Perfil", "Funcionalidad de edición de perfil en desarrollo.", "OK");
        }

        private async Task PerformLogout()
        {
            SecureStorage.Remove("jwt_token");
            SecureStorage.Remove("user_type");
            SecureStorage.Remove("user_id");

            await Application.Current.MainPage.DisplayAlert("Sesión Finalizada", "Has cerrado sesión exitosamente.", "OK");

            var loginPage = _serviceProvider.GetService<LoginPage>();
            if (loginPage != null)
            {
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
            else
            {
                Console.WriteLine("Error: LoginPage no pudo ser resuelta para cerrar sesión.");
            }
        }
    }
}
