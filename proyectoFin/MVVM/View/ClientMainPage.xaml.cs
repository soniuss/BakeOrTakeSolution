using Microsoft.Maui.Controls;
using proyectoFin.MVVM.ViewModel;
using System; // Necesario para IServiceProvider

namespace proyectoFin.MVVM.View
{
    public partial class ClientMainPage : FlyoutPage
    {
        private readonly IServiceProvider _serviceProvider; // Almacena el serviceProvider

        public ClientMainPage(ClientMainViewModel viewModel, IServiceProvider serviceProvider) // Inyecta IServiceProvider
        {
            InitializeComponent();
            this.BindingContext = viewModel;
            _serviceProvider = serviceProvider; // Asigna el serviceProvider

            IsPresented = false; // El menú no se muestra al inicio
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover; // El menú "flota" sobre el contenido

            // Asigna la página Detail inicial usando el ServiceProvider para inyectar dependencias
            Detail = new NavigationPage(_serviceProvider.GetRequiredService<RecipesPage>());
        }
    }
}