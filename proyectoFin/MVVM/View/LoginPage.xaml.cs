using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class LoginPage : ContentPage
{
	public LoginPage()
    {
        InitializeComponent();
        BindingContext = Application.Current.Handler.MauiContext.Services.GetService<LoginViewModel>();
    }
    // Manejador para el evento Clicked del boton "Registrate aqui"
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // Navega a la RegisterPage
        await Navigation.PushAsync(new RegisterPage());
    }
}