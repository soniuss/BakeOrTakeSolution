using proyectoFin.MVVM.ViewModel; // Asegúrate de tener este using

namespace proyectoFin.MVVM.View
{
    public partial class ProfilePage : ContentPage
    {
        // Constructor para inyección de dependencia
        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}