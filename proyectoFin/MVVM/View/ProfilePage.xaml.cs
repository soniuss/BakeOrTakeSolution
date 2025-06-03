using proyectoFin.MVVM.ViewModel;
using CommunityToolkit.Mvvm.Input; // Necesario para ExecuteAsync

namespace proyectoFin.MVVM.View
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Ejecutar el comando para cargar el perfil cuando la página aparece
            if (_viewModel.LoadUserProfileCommand.CanExecute(null))
            {
                await _viewModel.LoadUserProfileCommand.ExecuteAsync(null);
            }
        }
    }
}
