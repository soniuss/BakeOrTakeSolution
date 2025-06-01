using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // Para DisplayAlert, Application.Current.MainPage
using proyectoFin.Services; // Para IBakeOrTakeApi
using Domain.Model; // Para la entidad Empresa
using System.Net.Http; // Para posibles errores de HTTP
using System; // Para Exception, SecureStorage

namespace proyectoFin.MVVM.ViewModel
{
    // Es importante que sea partial para que CommunityToolkit.Mvvm genere el código
    public partial class EmpresaProfileViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

        // Propiedades para mostrar y editar los datos de la empresa
        // Usamos [ObservableProperty] para que se generen automáticamente las propiedades con NotifyPropertyChanged
        [ObservableProperty]
        private int _idEmpresa; // Para almacenar el ID de la empresa logueada

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _nombreNegocio;

        [ObservableProperty]
        private string _descripcion;

        [ObservableProperty]
        private string _ubicacion;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy; // Para indicar si una operación está en curso (ej. cargando/guardando)

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage; // Para mostrar mensajes de error a la UI

        // Comandos
        public IAsyncRelayCommand SaveChangesCommand { get; }
        public IAsyncRelayCommand LoadProfileCommand { get; } // Comando para cargar el perfil

        public EmpresaProfileViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;

            // Inicializar comandos
            SaveChangesCommand = new AsyncRelayCommand(SaveChanges);
            LoadProfileCommand = new AsyncRelayCommand(LoadEmpresaProfile);

            // Cargar el perfil cuando el ViewModel se inicializa
            // Esto es útil si la página se carga de nuevo o si el ViewModel es Transient
            _ = LoadEmpresaProfile();
        }

        // Método para cargar los datos del perfil de la empresa desde la API
        private async Task LoadEmpresaProfile()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // **Paso crucial: Obtener el ID de la empresa logueada**
                // Asume que el ID de la empresa logueada está guardado en SecureStorage.
                // Podrías tener un servicio de autenticación/sesión inyectado para esto también.
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int empresaId))
                {
                    ErrorMessage = "No se pudo obtener el ID de la empresa logueada.";
                    return;
                }
                IdEmpresa = empresaId; // Asigna el ID a la propiedad observable

                // **Llamada a la API para obtener los datos de la empresa**
                // Necesitarás añadir un endpoint en tu IBakeOrTakeApi para esto.
                // Ejemplo: [Get("/api/Empresas/{id}")] Task<ApiResponse<Empresa>> GetEmpresaByIdAsync(int id);
                var response = await _apiService.GetEmpresaByIdAsync(IdEmpresa); // Asume este método en tu API

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    // Asignar los datos al modelo observable
                    Email = response.Content.email;
                    NombreNegocio = response.Content.nombre_negocio;
                    Descripcion = response.Content.descripcion;
                    Ubicacion = response.Content.ubicacion;
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

        // Método para guardar los cambios en el perfil de la empresa
        private async Task SaveChanges()
        {
            if (IsBusy) return; // Evitar múltiples clics

            IsBusy = true;
            ErrorMessage = string.Empty;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(NombreNegocio) || string.IsNullOrWhiteSpace(Ubicacion) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "El nombre del negocio, la ubicación y el email son obligatorios.";
                IsBusy = false;
                return;
            }

            try
            {
                // Crear un DTO para la actualización. Podrías necesitar un EmpresaUpdateRequest si solo actualizas ciertos campos.
                // Por simplicidad, aquí usaremos la entidad Empresa directamente o un DTO genérico.
                // Si la API espera un objeto completo, crea una instancia de Empresa y asigna los valores actuales.
                var updateRequest = new Empresa
                {
                    id_empresa = IdEmpresa, // Asegúrate de enviar el ID para identificar qué empresa actualizar
                    email = Email,
                    nombre_negocio = NombreNegocio,
                    descripcion = Descripcion,
                    ubicacion = Ubicacion,
                    // No incluir password_hash ni fecha_registro a menos que realmente se modifiquen
                };

                // **Llamada a la API para actualizar los datos de la empresa**
                // Necesitarás añadir un endpoint en tu IBakeOrTakeApi para esto.
                // Ejemplo: [Put("/api/Empresas/{id}")] Task<ApiResponse<Empresa>> UpdateEmpresaAsync(int id, [Body] Empresa updateData);
                var response = await _apiService.UpdateEmpresaAsync(IdEmpresa, updateRequest); // Asume este método

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Perfil de empresa actualizado exitosamente.", "OK");
                    // Opcional: Recargar el perfil para asegurar que los datos estén actualizados
                    // _ = LoadEmpresaProfile();
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
    }
}