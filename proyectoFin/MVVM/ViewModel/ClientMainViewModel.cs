using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using proyectoFin.Services; // Si usas servicios API aquí
using proyectoFin.MVVM.View; // Para referenciar las Vistas

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ClientMainViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService; // Si lo necesitas
        private readonly IServiceProvider _serviceProvider; // Necesario para resolver las páginas

        public IRelayCommand NavigateToProfileCommand { get; }
        public IRelayCommand NavigateToMyRecipesCommand { get; }
        public IRelayCommand NavigateToFavoritesCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public ClientMainViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService; // Si lo necesitas
            _serviceProvider = serviceProvider;

            NavigateToProfileCommand = new RelayCommand(async () => await NavigateToPage<ProfilePage>());
            NavigateToMyRecipesCommand = new RelayCommand(async () => await NavigateToPage<MyRecipesPage>());
            NavigateToFavoritesCommand = new RelayCommand(async () => await NavigateToPage<FavoritesPage>());
            LogoutCommand = new RelayCommand(async () => await PerformLogout());

            // Aquí puedes cargar datos iniciales si es necesario para el dashboard principal
            // Por ejemplo, cargar las primeras recetas si no las gestiona RecipesViewModel
        }

        // Método genérico para navegar a una página desde el Flyout
        private async Task NavigateToPage<TPage>() where TPage : Page
        {
            // Resuelve la página del contenedor de inyección de dependencias
            var page = _serviceProvider.GetService<TPage>();

            if (page != null)
            {
                // Asegúrate de que la página principal es una FlyoutPage
                if (Application.Current.MainPage is FlyoutPage flyoutPage)
                {
                    // El Detail de la FlyoutPage DEBE ser una NavigationPage para PushAsync
                    if (flyoutPage.Detail is NavigationPage navigationPage)
                    {
                        // PopToRootAsync para limpiar la pila y que no se acumulen páginas
                        await navigationPage.PopToRootAsync();
                        await navigationPage.PushAsync(page);
                        flyoutPage.IsPresented = false; // Cierra el menú hamburguesa después de navegar
                    }
                    else
                    {
                        // Si Detail no es NavigationPage (no recomendado para navegación), reemplázala
                        flyoutPage.Detail = new NavigationPage(page);
                        flyoutPage.IsPresented = false;
                    }
                }
            }
            else
            {
                // Manejar error si la página no se pudo resolver
                Console.WriteLine($"Error: La página {typeof(TPage).Name} no pudo ser resuelta.");
                await Application.Current.MainPage.DisplayAlert("Error de navegación", $"No se pudo cargar la página {typeof(TPage).Name}.", "OK");
            }
        }

        private async Task PerformLogout()
        {
            // Lógica para cerrar sesión: limpiar preferencias de usuario, tokens, etc.
            // SecureStorage.Remove("jwt_token"); // Ejemplo si usas SecureStorage
            // Preferences.Remove("user_id");

            await Application.Current.MainPage.DisplayAlert("Sesión Finalizada", "Has cerrado sesión exitosamente.", "OK");

            // Navegar de vuelta a la página de inicio de sesión o bienvenida
            // Asegúrate de que LoginPage está registrada como AddTransient en MauiProgram.cs
            var loginPage = _serviceProvider.GetService<LoginPage>();
            if (loginPage != null)
            {
                Application.Current.MainPage = new NavigationPage(loginPage); // Establece LoginPage como la nueva raíz
            }
            else
            {
                Console.WriteLine("Error: LoginPage no pudo ser resuelta para cerrar sesión.");
            }
        }
    }
}