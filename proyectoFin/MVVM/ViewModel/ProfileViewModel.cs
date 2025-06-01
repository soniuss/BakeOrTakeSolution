using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain.Model; // ¡IMPORTANTE! Para tu clase Cliente
// Necesitarás un servicio para obtener los datos del cliente logueado
// using proyectoFin.Services.Auth; // Asume que tienes un servicio de autenticación para obtener el cliente actual

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty]
        private string userName; // Propiedad para el nombre del cliente

        [ObservableProperty]
        private string userEmail; // Propiedad para el email del cliente

        // Puedes añadir más propiedades de Cliente si las necesitas en la vista
        // [ObservableProperty]
        // private string userUbicacion;

        public IRelayCommand EditProfileCommand { get; }

        // Constructor
        // En una aplicación real, probablemente inyectarías un servicio de usuario/autenticación
        // para obtener los datos del cliente logueado. Por ahora, asumimos que puedes obtenerlos de algún lugar.
        public ProfileViewModel(/* IUserService userService */)
        {
            // _userService = userService;
            // Cargar datos del perfil al inicializar
            LoadUserProfile();

            EditProfileCommand = new AsyncRelayCommand(EditProfileAsync);
        }

        private void LoadUserProfile()
        {
            // En una aplicación real, aquí obtendrías el cliente logueado.
            // Podría ser desde SecureStorage, un servicio Singleton, etc.
            // Ejemplo rudimentario (NO USAR EN PRODUCCIÓN):
            // Cliente loggedInClient = _userService.GetCurrentClient(); // Ejemplo

            // Datos de ejemplo basados en tu clase Cliente
            UserName = "Juan Pérez (Cliente)"; // Usando la propiedad 'nombre' de Cliente
            UserEmail = "juan.perez@cliente.com"; // Usando la propiedad 'email' de Cliente
            // UserUbicacion = "Madrid"; // Si decides añadirla
        }

        private async Task EditProfileAsync()
        {
            await Application.Current.MainPage.DisplayAlert("Editar Perfil", "Funcionalidad de edición de perfil en desarrollo.", "OK");
        }
    }
}