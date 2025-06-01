using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // Para GetRequiredService

namespace proyectoFin.MVVM.View
{
    public partial class EmpresaProfilePage : ContentPage
    {
        public EmpresaProfilePage(EmpresaProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // Opcional: Si quieres recargar los datos cada vez que la página aparece
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is EmpresaProfileViewModel viewModel)
            {
                await viewModel.LoadProfileCommand.ExecuteAsync(null);
            }
        }
    }
}