using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // Necesario para Application.Current.MainPage
using proyectoFin.Services; // Asegúrate de que tu interfaz de API esté aquí
using Domain.Model.ApiRequests; // Asegúrate de que tus DTOs estén aquí
using proyectoFin.MVVM.View; // Necesario para ClientMainPage
using System; // Necesario para Exception, Console.WriteLine, IServiceProvider
using System.Net.Http; // Necesario para HttpResponseMessage si lo usas en el futuro


namespace proyectoFin.MVVM.ViewModel
{
    // Asegúrate de que hereda de ObservableObject
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider; // NUEVO: Para acceder al contenedor de DI

        // Constructor: Ahora inyecta IServiceProvider
        public RegisterViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider; // Asigna el IServiceProvider inyectado

            // Inicializa tus comandos
            PerformRegisterCommand = new AsyncRelayCommand(PerformRegister);
            SelectClientTypeCommand = new RelayCommand(() => IsClientSelected = true);
            SelectCompanyTypeCommand = new RelayCommand(() => IsClientSelected = false);
        }

        // --- Propiedades enlazables (recuerda que deben ser PascalCase) ---

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword; // Para la validación de la UI

        [ObservableProperty]
        private string nombre; // Se usará para nombre de cliente o nombre del negocio

        [ObservableProperty]
        private string ubicacion;

        [ObservableProperty]
        private string descripcion; // Solo para empresas (se usa con EmpresaRegistrationRequest)

        [ObservableProperty]
        private bool isClientSelected = true; // Por defecto, es cliente

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))] // CRUCIAL para IsNotBusy
        private bool isBusy; // Indica si hay una operación en curso (ej. llamada a la API)

        // Propiedad calculada para IsNotBusy
        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string errorMessage;

        // --- Comandos ---
        public IAsyncRelayCommand PerformRegisterCommand { get; }
        public IRelayCommand SelectClientTypeCommand { get; }
        public IRelayCommand SelectCompanyTypeCommand { get; }


        private async Task PerformRegister()
        {
            if (IsBusy) return; // Evitar múltiples clics si ya está ocupado

            IsBusy = true;
            ErrorMessage = string.Empty; // Limpiar mensaje de error anterior

            // --- Validaciones UI-level ---
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Ubicacion))
            {
                ErrorMessage = "Por favor, completa todos los campos requeridos.";
                IsBusy = false;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                IsBusy = false;
                return;
            }

            // Validación específica para empresas (si no es cliente, y la descripción está vacía)
            if (!IsClientSelected && string.IsNullOrWhiteSpace(Descripcion))
            {
                ErrorMessage = "Por favor, introduce la descripción del negocio.";
                IsBusy = false;
                return;
            }

            try
            {
                if (IsClientSelected)
                {
                    // Crear el Request DTO para Cliente
                    var request = new ClienteRegistrationRequest
                    {
                        Email = Email,
                        Password = Password,
                        Nombre = Nombre, // Mapea directamente a Nombre en ClienteRegistrationRequest
                        Ubicacion = Ubicacion
                    };

                    Refit.ApiResponse<Domain.Model.Cliente> clientApiResponse = await _apiService.RegisterClienteAsync(request);

                    if (clientApiResponse.IsSuccessStatusCode)
                    {
                        await Application.Current.MainPage.DisplayAlert("Éxito", "Registro de cliente completado.", "OK");
                        // Navegación a ClientMainPage usando el _serviceProvider inyectado
                        if (Application.Current.MainPage is NavigationPage navigationPage)
                        {
                            var clientMainPage = _serviceProvider.GetService<ClientTabsPage>();
                            if (clientMainPage != null)
                            {
                                await navigationPage.PushAsync(clientMainPage);
                            }
                            else
                            {
                                // Esto indica un problema con el registro de la página en MauiProgram.cs
                                Console.WriteLine("Error: ClientMainPage no pudo ser resuelta desde el contenedor de DI.");
                                ErrorMessage = "Error interno de la aplicación al navegar. Contacta soporte.";
                            }
                        }
                        else
                        {
                            // Fallback si MainPage no es una NavigationPage, reemplaza la página principal
                            var clientMainPage = _serviceProvider.GetService<ClientTabsPage>();
                            if (clientMainPage != null)
                            {
                                Application.Current.MainPage = clientMainPage;
                            }
                            else
                            {
                                Console.WriteLine("Error: ClientMainPage no pudo ser resuelta desde el contenedor de DI.");
                                ErrorMessage = "Error interno de la aplicación al navegar. Contacta soporte.";
                            }
                        }
                    }
                    else
                    {
                        // Manejo de errores específicos para cliente
                        var errorContent = clientApiResponse.Error?.Content; // Contenido del error de la API
                        ErrorMessage = $"Error al registrar cliente: {clientApiResponse.StatusCode}. Detalles: {errorContent}";
                        Console.WriteLine($"API Error Cliente: {clientApiResponse.StatusCode} - {errorContent}");
                    }
                }
                else // Empresa
                {
                    // Crear el Request DTO para Empresa
                    var request = new EmpresaRegistrationRequest
                    {
                        Email = Email,
                        Password = Password,
                        NombreNegocio = Nombre, // IMPORTANTE: Mapea ViewModel.Nombre a DTO.NombreNegocio
                        Descripcion = Descripcion,
                        Ubicacion = Ubicacion
                    };

                    Refit.ApiResponse<Domain.Model.Empresa> empresaApiResponse = await _apiService.RegisterEmpresaAsync(request);

                    if (empresaApiResponse.IsSuccessStatusCode)
                    {
                        await Application.Current.MainPage.DisplayAlert("Éxito", "Registro de empresa completado.", "OK");
                        // Navegación a ClientMainPage (o EmpresaMainPage si tuvieras una específica)
                        if (Application.Current.MainPage is NavigationPage navigationPage)
                        {
                            var empresaMainPage = _serviceProvider.GetService<EmpresaTabsPage>(); // O EmpresaMainPage
                            if (empresaMainPage != null)
                            {
                                await navigationPage.PushAsync(empresaMainPage);
                            }
                            else
                            {
                                Console.WriteLine("Error: EmpresaMainPage no pudo ser resuelta desde el contenedor de DI.");
                                ErrorMessage = "Error interno de la aplicación al navegar. Contacta soporte.";
                            }
                        }
                        else
                        {
                            // Fallback si MainPage no es una NavigationPage
                            var empresaMainPage = _serviceProvider.GetService<EmpresaTabsPage>(); // O EmpresaMainPage
                            if (empresaMainPage != null)
                            {
                                Application.Current.MainPage = empresaMainPage;
                            }
                            else
                            {
                                Console.WriteLine("Error: EmpresaMainPage no pudo ser resuelta desde el contenedor de DI.");
                                ErrorMessage = "Error interno de la aplicación al navegar. Contacta soporte.";
                            }
                        }
                    }
                    else
                    {
                        // Manejo de errores específicos para empresa
                        var errorContent = empresaApiResponse.Error?.Content;
                        ErrorMessage = $"Error al registrar empresa: {empresaApiResponse.StatusCode}. Detalles: {errorContent}";
                        Console.WriteLine($"API Error Empresa: {empresaApiResponse.StatusCode} - {errorContent}");
                    }
                }
            }
            catch (Refit.ApiException ex)
            {
                // Capturar errores específicos de Refit (errores HTTP del servidor, problemas de conexión)
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                // Capturar cualquier otra excepción inesperada
                ErrorMessage = $"Ocurrió un error inesperado: {ex.Message}";
                Console.WriteLine($"General Exception in RegisterViewModel: {ex}");
            }
            finally
            {
                IsBusy = false; // Asegurarse de desactivar el indicador de actividad
            }
        }
    }
}