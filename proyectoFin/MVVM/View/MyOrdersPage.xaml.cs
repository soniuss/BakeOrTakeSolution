using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class MyOrdersPage : ContentPage
{
    public MyOrdersPage(MyOrdersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}