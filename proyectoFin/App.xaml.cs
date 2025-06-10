using proyectoFin.MVVM.View;

namespace proyectoFin
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MainPage = new NavigationPage(serviceProvider.GetRequiredService<WelcomePage>());
        }
    }
}
