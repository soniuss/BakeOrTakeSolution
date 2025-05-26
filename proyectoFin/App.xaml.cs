using proyectoFin.MVVM.View;

namespace proyectoFin
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Obtén la WelcomePage del contenedor de servicios
            // Esto asegura que WelcomePage y su ViewModel se inyecten correctamente.
            MainPage = new NavigationPage(serviceProvider.GetRequiredService<WelcomePage>());
        }
    }
}
