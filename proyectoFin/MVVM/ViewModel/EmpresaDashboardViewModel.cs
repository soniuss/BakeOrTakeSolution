using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain.Model;
using proyectoFin.MVVM.View; // Para referenciar las Vistas
using System; // Para IServiceProvider

namespace proyectoFin.MVVM.ViewModel
{
    public partial class EmpresaDashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string welcomeMessage;

        private readonly IServiceProvider _serviceProvider; // Inyectar IServiceProvider

        public IRelayCommand ViewOrdersCommand { get; }
        public IRelayCommand ManageRecipesCommand { get; }

        public EmpresaDashboardViewModel(IServiceProvider serviceProvider) // Recibir IServiceProvider
        {
            _serviceProvider = serviceProvider;
            WelcomeMessage = "Bienvenido a tu panel de gestión empresarial.";

            ViewOrdersCommand = new AsyncRelayCommand(async () => await ViewOrdersAsync());
            ManageRecipesCommand = new AsyncRelayCommand(async () => await ManageRecipesAsync());
        }

        private async Task ViewOrdersAsync()
        {
            await Application.Current.MainPage.DisplayAlert("Mis Pedidos", "Mostrando la lista de pedidos.", "OK");
            // **CORRECCIÓN DE NAVEGACIÓN**
            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var ordersPage = _serviceProvider.GetService<MyOrdersPage>(); // Asume que MyOrdersPage existe
                if (ordersPage != null)
                {
                    await navigationPage.PopToRootAsync(); // Limpia la pila para evitar duplicados
                    await navigationPage.PushAsync(ordersPage);
                    flyoutPage.IsPresented = false;
                }
                else
                {
                    Console.WriteLine("Error: MyOrdersPage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página de pedidos.", "OK");
                }
            }
        }

        private async Task ManageRecipesAsync()
        {
            await Application.Current.MainPage.DisplayAlert("Gestionar Recetas", "Accediendo a la gestión de recetas.", "OK");
           
            if (Application.Current.MainPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
            {
                var manageRecipesPage = _serviceProvider.GetService<ManageRecipesPage>(); // Asume que ManageRecipesPage existe
                if (manageRecipesPage != null)
                {
                    await navigationPage.PopToRootAsync(); // Limpia la pila
                    await navigationPage.PushAsync(manageRecipesPage);
                    flyoutPage.IsPresented = false;
                }
                else
                {
                    Console.WriteLine("Error: ManageRecipesPage no pudo ser resuelta.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página de gestión de recetas.", "OK");
                }
            }
        }
    }
}