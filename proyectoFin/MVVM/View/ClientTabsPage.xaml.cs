// En proyectoFin/MVVM/View/ClientTabsPage.xaml.cs
using Microsoft.Maui.Controls;
using System; // Necesario para IServiceProvider
using Microsoft.Extensions.DependencyInjection; // Necesario para GetService
using proyectoFin.MVVM.ViewModel; // Si ClientTabsPage necesitara su propio ViewModel

namespace proyectoFin.MVVM.View
{
    public partial class ClientTabsPage : TabbedPage
    {
        private readonly IServiceProvider _serviceProvider; // Inyecta IServiceProvider

        public ClientTabsPage(IServiceProvider serviceProvider) // Recibe IServiceProvider
        {
            InitializeComponent();
            _serviceProvider = serviceProvider; // Almacena el serviceProvider

            // A�adir las p�ginas como hijos de la TabbedPage
            // �Aseg�rate de que tus ViewModels y Pages est�n registrados como Transient en MauiProgram.cs!

            // Pesta�a 1: Explorar Recetas
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<RecipesPage>())
            {
                Title = "Explorar",
                IconImageSource = "hoja.png" 
            });

            // Pesta�a 2: Mis Recetas
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<MyRecipesPage>())
            {
                Title = "Mis Recetas",
                IconImageSource = "recetas.png"
            });

            // Pesta�a 3: Mis Pedidos (MyOrdersPage) - �NUEVA PESTA�A PARA CLIENTE!
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<MyOrdersPage>())
            {
                Title = "Mis Pedidos",
                IconImageSource = "pedido.png" // Reutilizamos el icono de pedidos
            });

            // Pesta�a 4: Favoritos
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<FavoritesPage>())
            {
                Title = "Favoritos",
                IconImageSource = "favorito.png"
            });

            // Pesta�a 5: Mi Perfil
            Children.Add(new NavigationPage(_serviceProvider.GetRequiredService<ProfilePage>())
            {
                Title = "Mi Perfil",
                IconImageSource = "perfil.png" 
            });
        }
    }
}