using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using proyectoFin.Services; 
using proyectoFin.MVVM.View; 

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ClientMainViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService; 
        private readonly IServiceProvider _serviceProvider; 

        public IRelayCommand NavigateToProfileCommand { get; }
        public IRelayCommand NavigateToMyRecipesCommand { get; }
        public IRelayCommand NavigateToFavoritesCommand { get; }
        public IRelayCommand LogoutCommand { get; }
        public IRelayCommand ToggleFlyoutCommand { get; } 

        public ClientMainViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {

            ToggleFlyoutCommand = new RelayCommand(ToggleFlyout); // Inicializar el comando
            _apiService = apiService; 
            _serviceProvider = serviceProvider;

            NavigateToProfileCommand = new RelayCommand(async () => await NavigateToPage<ProfilePage>());
            NavigateToMyRecipesCommand = new RelayCommand(async () => await NavigateToPage<MyRecipesPage>());
            NavigateToFavoritesCommand = new RelayCommand(async () => await NavigateToPage<FavoritesPage>());
            LogoutCommand = new RelayCommand(async () => await PerformLogout());

            
        }
        private void ToggleFlyout() // Método que abre/cierra el Flyout
        {
            if (Application.Current.MainPage is FlyoutPage flyoutPage)
            {
                flyoutPage.IsPresented = !flyoutPage.IsPresented;
            }
        }

        private async Task NavigateToPage<TPage>() where TPage : Page
        {
            // Resuelve la página del contenedor de inyección de dependencias
            var page = _serviceProvider.GetService<TPage>();

            if (page != null)
            {
                if (Application.Current.MainPage is FlyoutPage flyoutPage)
                {
                    if (flyoutPage.Detail is NavigationPage navigationPage)
                    {
                        await navigationPage.PopToRootAsync();
                        await navigationPage.PushAsync(page);
                        flyoutPage.IsPresented = false; 
                    }
                    else
                    {
                        
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