using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using proyectoFin.MVVM.Model.ApiRequests;
using proyectoFin.Services;
using Refit;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _nombre; // Para Cliente y Empresa (nombre de negocio)

        [ObservableProperty]
        private string _descripcion; // Solo para Empresa

        [ObservableProperty]
        private string _ubicacion; // Para Cliente y Empresa

        [ObservableProperty]
        private bool _isClientSelected = true; // Por defecto, Cliente

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        public RegisterViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private void SelectClientType() => IsClientSelected = true;

        [RelayCommand]
        private void SelectCompanyType() => IsClientSelected = false;

        [RelayCommand]
        private async Task PerformRegister()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword) || string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Ubicacion))
            {
                ErrorMessage = "Por favor, completa todos los campos obligatorios.";
                IsBusy = false;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                IsBusy = false;
                return;
            }

            try
            {
                if (IsClientSelected)
                {
                    var request = new ClienteRegistrationRequest
                    {
                        Email = Email,
                        Password = Password,
                        Nombre = Nombre,
                        Ubicacion = Ubicacion
                    };
                    var response = await _apiService.RegisterClienteAsync(request);

                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        Console.WriteLine($"Registro de cliente exitoso. ID: {response.Content.id_cliente}");
                        // Vuelve a la pagina anterior (Login o Welcome)
                        await Application.Current.MainPage.Navigation.PopAsync();
                        // Opcional: Mostrar un mensaje de exito al usuario (ej. DisplayAlert)
                    }
                    else
                    {
                        ErrorMessage = $"Error al registrar cliente: {response.ReasonPhrase}";
                        if (response.Error != null && !string.IsNullOrEmpty(response.Error.Content))
                        {
                            Console.WriteLine($"Detalles del error API: {response.Error.Content}");
                        }
                    }
                }
                else // Empresa
                {
                    if (string.IsNullOrWhiteSpace(Descripcion))
                    {
                        ErrorMessage = "La descripción del negocio es obligatoria para empresas.";
                        IsBusy = false;
                        return;
                    }

                    var request = new EmpresaRegistrationRequest
                    {
                        Email = Email,
                        Password = Password,
                        NombreNegocio = Nombre, // Usamos Nombre para NombreNegocio
                        Descripcion = Descripcion,
                        Ubicacion = Ubicacion
                    };
                    var response = await _apiService.RegisterEmpresaAsync(request);

                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        Console.WriteLine($"Registro de empresa exitoso. ID: {response.Content.id_empresa}");
                        // Vuelve a la pagina anterior (Login o Welcome)
                        await Application.Current.MainPage.Navigation.PopAsync();
                        // Opcional: Mostrar un mensaje de exito al usuario
                    }
                    else
                    {
                        ErrorMessage = $"Error al registrar empresa: {response.ReasonPhrase}";
                        if (response.Error != null && !string.IsNullOrEmpty(response.Error.Content))
                        {
                            Console.WriteLine($"Detalles del error API: {response.Error.Content}");
                        }
                    }
                }
            }
            catch (ApiException ex)
            {
                ErrorMessage = $"Error de red o API: {ex.Message}";
                Console.WriteLine($"API Exception: {ex}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado: {ex.Message}";
                Console.WriteLine($"General Exception: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
