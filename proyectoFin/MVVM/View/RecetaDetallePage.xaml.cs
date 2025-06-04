using Microsoft.Maui.Controls;
using proyectoFin.MVVM.ViewModel; // Asegúrate de tener este using

namespace proyectoFin.MVVM.View
{
    public partial class RecetaDetallePage : ContentPage
    {
        private readonly RecetaDetalleViewModel _viewModel;

        public RecetaDetallePage(RecetaDetalleViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Esto asegura que el ViewModel carga los datos cuando la página se hace visible
            if (_viewModel.LoadRecetaCommand.CanExecute(null))
            {
                await _viewModel.LoadRecetaCommand.ExecuteAsync(null);
            }
        }
    }
}