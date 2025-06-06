using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain.Model; // Para la entidad Cliente
using proyectoFin.Services; // Para IBakeOrTakeApi
using Microsoft.Maui.Storage; // Para SecureStorage
using System; // Para Exception, IServiceProvider
using Refit; // Para ApiException
using proyectoFin.MVVM.View; // Para LoginPage (en LogoutCommand)

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userEmail;

        [ObservableProperty]
        private string userUbicacion;

        // ¡NUEVO! Propiedad para controlar el modo de edición
        [ObservableProperty]
        private bool _isEditing;

        // Propiedad calculada para el estado de "no ocupado"
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        // Comandos
        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IAsyncRelayCommand LoadUserProfileCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        // ¡NUEVO! Comando para alternar el modo de edición
        public IRelayCommand ToggleEditModeCommand { get; }

        public ProfileViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            SaveChangesCommand = new AsyncRelayCommand(SaveChanges);
            LoadUserProfileCommand = new AsyncRelayCommand(LoadUserProfile);
            LogoutCommand = new RelayCommand(async () => await PerformLogout());

            // ¡NUEVO! Inicializar el comando de alternar edición
            ToggleEditModeCommand = new RelayCommand(ToggleEditMode);

            _ = LoadUserProfile(); // Cargar perfil al inicializar
        }

        // ¡NUEVO! Método para alternar el modo de edición
        private void ToggleEditMode()
        {
            IsEditing = !IsEditing;
            // Si salimos del modo edición sin guardar, recargar los datos originales
            if (!IsEditing)
            {
                _ = LoadUserProfile(); // Recargar para descartar cambios no guardados
            }
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
                    ErrorMessage = "No se pudo obtener el token de autenticación para cargar el perfil. Por favor, intente iniciar sesión de nuevo si el problema persiste.";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                    IsBusy = false;
                    return;
                }

                // Llamada a la API para obtener los datos del cliente
                var response = await _apiService.GetClienteByIdAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var cliente = response.Content; // El tipo es Domain.Model.Cliente
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

        // ¡NUEVO! Método para guardar los cambios del perfil de cliente
        private async Task SaveChanges()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserUbicacion) || string.IsNullOrWhiteSpace(UserEmail))
            {
                ErrorMessage = "El nombre, la ubicación y el email son obligatorios.";
                IsBusy = false;
                return;
            }

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del cliente logueado.";
                    IsBusy = false;
                    return;
                }

                // Crear un objeto Cliente para enviar a la API con los campos actualizados
                // La API debería tener un endpoint PUT /api/Clientes/{id}
                var updateRequest = new Cliente
                {
                    id_cliente = idClienteActual,
                    email = UserEmail, // El email no suele cambiarse, pero si la API lo acepta...
                    nombre = UserName,
                    ubicacion = UserUbicacion
                    // No incluir password_hash ni fecha_registro aquí
                };

                // Llama al endpoint de la API para actualizar el cliente
                // Necesitarás un método UpdateClienteAsync en IBakeOrTakeApi.cs
                // Y un endpoint PUT /api/Clientes/{id} en ClientesController.cs
                var response = await _apiService.UpdateClienteAsync(idClienteActual, updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Perfil actualizado exitosamente.", "OK");
                    IsEditing = false; // Salir del modo edición
                    _ = LoadUserProfile(); // Recargar datos para asegurar consistencia
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al guardar cambios: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al guardar perfil de cliente: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al guardar perfil: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al guardar el perfil: {ex.Message}";
                Console.WriteLine($"General Exception en SaveChanges: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // El LogoutCommand ya existe y funciona.
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
