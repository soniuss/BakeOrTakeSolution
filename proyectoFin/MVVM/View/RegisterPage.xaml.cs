using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Manejador para el evento Clicked del boton "Inicia sesion aqui"
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        
        await Navigation.PopAsync();
    }
}