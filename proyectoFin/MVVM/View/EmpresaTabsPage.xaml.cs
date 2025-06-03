using Microsoft.Maui.Controls;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace proyectoFin.MVVM.View
{
    public partial class EmpresaTabsPage : TabbedPage
    {
        private readonly IServiceProvider _serviceProvider;

        public EmpresaTabsPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // A�adir las p�ginas como hijos de la TabbedPage program�ticamente para DI

            // Pesta�a 1: Mis Pedidos (anteriormente Dashboard era la primera)
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<MyOrdersPage>())
            {
                Title = "Pedidos",
                IconImageSource = "pedido.png" // Aseg�rate de tener este icono
            });

            // Pesta�a 2: Gestionar Recetas
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<ManageRecipesPage>())
            {
                Title = "Gestionar Recetas",
                IconImageSource = "recetas.png" // Aseg�rate de tener este icono
            });

            // Pesta�a 3: Perfil de Empresa (ahora usando CompanyProfilePage)
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<CompanyProfilePage>()) // �CAMBIO AQU�!
            {
                Title = "Mi Perfil",
                IconImageSource = "perfil.png" // Aseg�rate de tener este icono
            });
        }
    }
}
