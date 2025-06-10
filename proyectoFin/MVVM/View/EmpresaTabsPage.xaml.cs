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

                // Pestaña 1: Explorar Recetas (para empresas)
                Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<RecipesPage>()) 
                {
                    Title = "Explorar",
                    IconImageSource = "hoja.png" // Reutilizar el icono de explorar
                });

                // Pestaña 2: Mis Pedidos
                Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<MyOrdersPage>())
                {
                    Title = "Pedidos",
                    IconImageSource = "pedido.png"
                });

                // Pestaña 3: Perfil de Empresa
                Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<CompanyProfilePage>())
                {
                    Title = "Mi Perfil",
                    IconImageSource = "perfil.png"
                });
            }
        }
    }
    