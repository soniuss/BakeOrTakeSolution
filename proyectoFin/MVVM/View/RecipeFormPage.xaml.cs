using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // Para GetRequiredService
using System.Collections.Generic; // Para IDictionary

namespace proyectoFin.MVVM.View
{
    public partial class RecipeFormPage : ContentPage
    {
        private readonly RecipeFormViewModel _viewModel; // Almacena una referencia al ViewModel

        public RecipeFormPage(RecipeFormViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel; // Asigna el ViewModel inyectado
            BindingContext = _viewModel;
        }

        // OnAppearing es el lugar ideal para inicializar el ViewModel con datos
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            
            await _viewModel.InitializeAsync(_viewModel.RecetaId); // Re-inicializa el VM si el ID cambió o para asegurar carga.
        }
    }
}