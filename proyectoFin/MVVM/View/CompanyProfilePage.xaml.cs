using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // Solo si lo usas para resolver servicios aquí

namespace proyectoFin.MVVM.View
{
    public partial class CompanyProfilePage : ContentPage
    {
        // Almacenar una referencia al ViewModel para el OnAppearing
        private readonly EmpresaProfileViewModel _viewModel;

        public CompanyProfilePage(EmpresaProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel; // Asigna el ViewModel inyectado
            BindingContext = _viewModel;
        }

        // OnAppearing es el lugar ideal para cargar los datos del perfil
        // cada vez que la página se hace visible.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Ejecuta el comando para cargar el perfil de la empresa
            await _viewModel.LoadProfileCommand.ExecuteAsync(null);
        }
    }
}