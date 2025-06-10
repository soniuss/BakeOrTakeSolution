using proyectoFin.MVVM.ViewModel;

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
            if (_viewModel.LoadMyRecipesCommand.CanExecute(null))
            {
                await _viewModel.LoadMyRecipesCommand.ExecuteAsync(null);
            }
        }
    }
}
