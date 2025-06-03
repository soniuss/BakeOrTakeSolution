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

            // Añadir las páginas como hijos de la TabbedPage programáticamente para DI

            // Pestaña 1: Mis Pedidos (anteriormente Dashboard era la primera)
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<MyOrdersPage>())
            {
                Title = "Pedidos",
                IconImageSource = "pedido.png" // Asegúrate de tener este icono
            });

            // Pestaña 2: Gestionar Recetas
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<ManageRecipesPage>())
            {
                Title = "Gestionar Recetas",
                IconImageSource = "recetas.png" // Asegúrate de tener este icono
            });

            // Pestaña 3: Perfil de Empresa (ahora usando CompanyProfilePage)
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<CompanyProfilePage>()) // ¡CAMBIO AQUÍ!
            {
                Title = "Mi Perfil",
                IconImageSource = "perfil.png" // Asegúrate de tener este icono
            });
        }
    }
}
