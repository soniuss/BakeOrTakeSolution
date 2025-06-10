using proyectoFin.MVVM.ViewModel;
using CommunityToolkit.Mvvm.Input;

namespace proyectoFin.MVVM.View
{
    public partial class FavoritesPage : ContentPage
    {
        private readonly FavoritesViewModel _viewModel;

        public FavoritesPage(FavoritesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.LoadFavoriteRecipesCommand is IAsyncRelayCommand asyncCommand && asyncCommand.CanExecute(null))
            {
                await asyncCommand.ExecuteAsync(null);
            }
        }
    }
}
