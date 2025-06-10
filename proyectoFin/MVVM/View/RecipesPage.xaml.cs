using proyectoFin.MVVM.ViewModel;
using CommunityToolkit.Mvvm.Input; // Asegúrate de tener este using

namespace proyectoFin.MVVM.View
{
    public partial class RecipesPage : ContentPage
    {
        private readonly RecipesViewModel _viewModel;

        public RecipesPage(RecipesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.LoadRecipesCommand is IAsyncRelayCommand asyncCommand && asyncCommand.CanExecute(null))
            {
                await asyncCommand.ExecuteAsync(null);
            }
        }
    }
}
