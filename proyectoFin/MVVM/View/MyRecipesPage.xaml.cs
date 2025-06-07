using proyectoFin.MVVM.ViewModel;
using CommunityToolkit.Mvvm.Input; // Para ExecuteAsync
using Microsoft.Maui.Controls; // Para ContentPage

namespace proyectoFin.MVVM.View
{
    public partial class MyRecipesPage : ContentPage
    {
        private readonly MyRecipesViewModel _viewModel;

        public MyRecipesPage(MyRecipesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // �CORRECCI�N CLAVE AQU�! Recargar los datos cuando la p�gina aparece
            // Esto es lo que deber�a hacer que la nueva receta aparezca.
            if (_viewModel.LoadMyRecipesCommand.CanExecute(null))
            {
                await _viewModel.LoadMyRecipesCommand.ExecuteAsync(null);
            }
        }
    }
}
