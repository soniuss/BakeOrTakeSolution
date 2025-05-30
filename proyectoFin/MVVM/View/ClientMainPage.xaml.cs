using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class ClientMainPage : ContentPage
{
    public ClientMainPage(ClientMainViewModel viewModel) 
    {
        InitializeComponent();
        BindingContext = viewModel; 
    }
}