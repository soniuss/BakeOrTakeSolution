using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using proyectoFin.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System;
using proyectoFin.MVVM.View;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class EmpresaMainViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider; // Necesario para cerrar sesion

        [ObservableProperty]
        private string _welcomeMessage; // Ejemplo de propiedad

        public EmpresaMainViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            LoadWelcomeMessage(); // Llama a un método para cargar un mensaje inicial
        }

        private async void LoadWelcomeMessage()
        {
            // Aquí podrías cargar datos específicos de la empresa, como su nombre
            // a partir del SecureStorage (si guardaste el nombre o puedes obtenerlo con el ID)
            var userName = await SecureStorage.GetAsync("user_name") ?? "Empresa"; // Asumiendo que guardaste el nombre del negocio
            WelcomeMessage = $"¡Bienvenido, {userName}!";
        }

        [RelayCommand]
        private async Task CerrarSesion()
        {
            try
            {
                await SecureStorage.SetAsync("jwt_token", null);
                await SecureStorage.SetAsync("user_type", null);
                await SecureStorage.SetAsync("user_id", null);
                await SecureStorage.SetAsync("user_name", null); // Si también guardaste el nombre
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error limpiando SecureStorage al cerrar sesión de empresa: {ex.Message}");
            }

            // Redirige a la página de bienvenida (login/registro)
            var welcomePage = _serviceProvider.GetService<WelcomePage>();
            if (welcomePage != null)
            {
                Application.Current.MainPage = new NavigationPage(welcomePage);
            }
            else
            {
                // Fallback si no se puede resolver WelcomePage (idealmente no debería pasar)
                Application.Current.MainPage = new NavigationPage(
                    new WelcomePage(
                        new WelcomeViewModel(
                            _serviceProvider.GetService<IBakeOrTakeApi>(),
                            _serviceProvider
                        )
                    )
                );
            }
        }

        // Puedes añadir más comandos o propiedades según las funcionalidades de la empresa:
        // [RelayCommand]
        // private async Task GoToMisOfertas() { /* ... */ }

        // [RelayCommand]
        // private async Task GoToGestionarPedidos() { /* ... */ }
    }
}