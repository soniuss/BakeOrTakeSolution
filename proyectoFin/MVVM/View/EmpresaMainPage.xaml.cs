using Microsoft.Maui.Controls;
using proyectoFin.MVVM.ViewModel;
using System; // Necesario para IServiceProvider

namespace proyectoFin.MVVM.View
{
    public partial class EmpresaMainPage : FlyoutPage
    {
        private readonly IServiceProvider _serviceProvider; // Almacena el serviceProvider

        public EmpresaMainPage(EmpresaMainViewModel viewModel, IServiceProvider serviceProvider) // Inyecta IServiceProvider
        {
            InitializeComponent();
            this.BindingContext = viewModel;
            _serviceProvider = serviceProvider; // Asigna el serviceProvider

            // Asigna la página Detail inicial usando el ServiceProvider para inyectar dependencias
            Detail = new NavigationPage(_serviceProvider.GetRequiredService<EmpresaDashboardContentPage>());
        }
    }
}