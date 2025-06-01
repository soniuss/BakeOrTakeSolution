using proyectoFin.MVVM.ViewModel; // Aseg�rate de tener este using

namespace proyectoFin.MVVM.View
{
    public partial class ProfilePage : ContentPage
    {
        // Constructor para inyecci�n de dependencia
        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}