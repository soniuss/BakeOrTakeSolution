using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model; 
using proyectoFin.Services; 
using proyectoFin.MVVM.View; 

namespace proyectoFin.MVVM.ViewModel
{
    public partial class EmpresaProfileViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _nombreNegocio;

        [ObservableProperty]
        private string _descripcion;

        [ObservableProperty]
        private string _ubicacion;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IAsyncRelayCommand LoadProfileCommand { get; }
        public IRelayCommand LogoutCommand { get; }
        public IRelayCommand ToggleEditModeCommand { get; }

        public EmpresaProfileViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            SaveChangesCommand = new AsyncRelayCommand(SaveChanges);
            LoadProfileCommand = new AsyncRelayCommand(LoadEmpresaProfile);
            LogoutCommand = new RelayCommand(async () => await PerformLogout());
            ToggleEditModeCommand = new RelayCommand(ToggleEditMode);

            _ = LoadEmpresaProfile();
        }

        private void ToggleEditMode()
        {
            IsEditing = !IsEditing;
            if (!IsEditing)
            {
                _ = LoadEmpresaProfile();
            }
        }

        private async Task LoadEmpresaProfile()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idEmpresaActual))
                {
                    ErrorMessage = "No se pudo obtener el ID de la empresa logueada.";
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
                        Console.WriteLine($"DEBUG: Token no encontrado aún en EmpresaProfileViewModel. Reintentando... (Intento {retryCount + 1})");
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

               
                var response = await _apiService.GetEmpresaByIdAsync(idEmpresaActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var empresa = response.Content;
                    Email = empresa.email;
                    NombreNegocio = empresa.nombre_negocio;
                    Descripcion = empresa.descripcion;
                    Ubicacion = empresa.ubicacion;
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al cargar perfil: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al cargar perfil de empresa: {response.StatusCode} - {errorContent}");
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
                Console.WriteLine($"General Exception en LoadEmpresaProfile: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveChanges()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NombreNegocio) || string.IsNullOrWhiteSpace(Ubicacion) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "El nombre del negocio, la ubicación y el email son obligatorios.";
                IsBusy = false;
                return;
            }

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idEmpresaActual))
                {
                    ErrorMessage = "No se pudo obtener el ID de la empresa logueada.";
                    IsBusy = false;
                    return;
                }

                var updateRequest = new Empresa
                {
                    id_empresa = idEmpresaActual,
                    email = Email,
                    nombre_negocio = NombreNegocio,
                    descripcion = Descripcion,
                    ubicacion = Ubicacion,
                };

                
                var response = await _apiService.UpdateEmpresaAsync(idEmpresaActual, updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Perfil de empresa actualizado exitosamente.", "OK");
                    IsEditing = false;
                    _ = LoadEmpresaProfile();
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al guardar cambios: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al guardar perfil de empresa: {response.StatusCode} - {errorContent}");
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
