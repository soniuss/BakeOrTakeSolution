// En proyectoFin/MVVM/View/EmpresaTabsPage.xaml.cs
using Microsoft.Maui.Controls;
using System; // Necesario para IServiceProvider
using Microsoft.Extensions.DependencyInjection; // Necesario para GetRequiredService

namespace proyectoFin.MVVM.View
{
    public partial class EmpresaTabsPage : TabbedPage
    {
        private readonly IServiceProvider _serviceProvider; // Inyecta IServiceProvider

        public EmpresaTabsPage(IServiceProvider serviceProvider) // Recibe IServiceProvider
        {
            InitializeComponent();
            _serviceProvider = serviceProvider; // Almacena el serviceProvider

            // Pestaña 1: Dashboard de Empresa
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<EmpresaDashboardContentPage>())
            {
                Title = "Dashboard",
                IconImageSource = "dashboard_icon.png" // Asegúrate de tener este icono
            });

            // Pestaña 2: Mis Pedidos
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<MyOrdersPage>())
            {
                Title = "Pedidos",
                IconImageSource = "orders_icon.png" // Asegúrate de tener este icono
            });

            // Pestaña 3: Gestionar Recetas
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<ManageRecipesPage>())
            {
                Title = "Gestionar Recetas",
                IconImageSource = "manage_recipes_icon.png" // Asegúrate de tener este icono
            });

            // Pestaña 4: Perfil de Empresa
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<CompanyProfilePage>())
            {
                Title = "Mi Perfil",
                IconImageSource = "company_profile_icon.png" // Asegúrate de tener este icono
            });
        }
    }
}