using proyectoFin.MVVM.ViewModel;
using CommunityToolkit.Mvvm.Input; // Para ExecuteAsync

namespace proyectoFin.MVVM.View
{
    public partial class CompanyProfilePage : ContentPage
    {
        private readonly EmpresaProfileViewModel _viewModel;

        public CompanyProfilePage(EmpresaProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.LoadProfileCommand.CanExecute(null))
            {
                await _viewModel.LoadProfileCommand.ExecuteAsync(null);
            }
        }
    }
}
