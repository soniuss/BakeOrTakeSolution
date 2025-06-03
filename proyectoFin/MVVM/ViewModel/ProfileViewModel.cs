using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain.Model; // ¡NUEVO! Para la entidad Cliente
using proyectoFin.Services; // Para IBakeOrTakeApi
using Microsoft.Maui.Storage; // Para SecureStorage
using System;
using proyectoFin.MVVM.View; // Para Exception, IServiceProvider

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider; // Necesario para navegar al logout

        [ObservableProperty]
        private string userName; // Propiedad para el nombre del cliente

        [ObservableProperty]
        private string userEmail; // Propiedad para el email del cliente

        [ObservableProperty]
        private string userUbicacion; // Nuevo: Propiedad para la ubicación del cliente

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public IRelayCommand EditProfileCommand { get; }
        public IRelayCommand LogoutCommand { get; }
        public IAsyncRelayCommand LoadUserProfileCommand { get; } // Nuevo comando para cargar el perfil

        public ProfileViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            EditProfileCommand = new AsyncRelayCommand(EditProfileAsync);
            LogoutCommand = new RelayCommand(async () => await PerformLogout());
            LoadUserProfileCommand = new AsyncRelayCommand(LoadUserProfile); // Inicializar el comando

            // Cargar datos del perfil al inicializar el ViewModel
            _ = LoadUserProfile();
        }

        private async Task LoadUserProfile()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // **Paso crucial: Obtener el ID del cliente logueado desde SecureStorage**
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado.";
                    return;
                }

                // --- Lógica de espera/reintento para el token ---
                string token = await SecureStorage.GetAsync("jwt_token");
                int retryCount = 0;
                const int maxRetries = 3;
                const int retryDelayMs = 500;

                if (string.IsNullOrEmpty(token))
                {
                    while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                    {
                        Console.WriteLine($"DEBUG: Token no encontrado aún en ProfileViewModel. Reintentando... (Intento {retryCount + 1})");
                        await Task.Delay(retryDelayMs);
                        token = await SecureStorage.GetAsync("jwt_token");
                        retryCount++;
                    }
                }

                if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
                {
                    ErrorMessage = "No se pudo obtener el token de autenticación para cargar el perfil. Por favor, intente iniciar sesión de nuevo.";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                    IsBusy = false;
                    return;
                }
                // --- FIN LÓGICA DE ESPERA ---


                // Llamada a la API para obtener los datos del cliente
                var response = await _apiService.GetClienteByIdAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var cliente = response.Content; // El tipo es Domain.Model.Cliente
                    UserName = cliente.nombre;
                    UserEmail = cliente.email;
                    UserUbicacion = cliente.ubicacion; // Asignar ubicación
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
            // Aquí podrías navegar a una página de edición de perfil, pasando el ID del cliente
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
