using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input; // Asegúrate de tener este using

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
            // CORRECCIÓN: Castear a IAsyncRelayCommand antes de llamar ExecuteAsync
            if (_viewModel.LoadFavoriteRecipesCommand is IAsyncRelayCommand asyncCommand && asyncCommand.CanExecute(null))
            {
                await asyncCommand.ExecuteAsync(null);
            }
        }
    }
}
