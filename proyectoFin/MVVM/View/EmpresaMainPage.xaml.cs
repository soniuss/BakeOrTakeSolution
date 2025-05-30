using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class EmpresaMainPage : ContentPage
{
    // Constructor que recibe el ViewModel a través de la inyección de dependencias
    public EmpresaMainPage(EmpresaMainViewModel viewModel) // Necesitas crear EmpresaMainViewModel
    {
        InitializeComponent();
        BindingContext = viewModel; // Asigna el ViewModel al BindingContext
    }
}