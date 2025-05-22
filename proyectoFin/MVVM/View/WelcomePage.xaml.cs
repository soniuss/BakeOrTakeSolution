using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class WelcomePage : ContentPage
{
	public WelcomePage()
	{
		InitializeComponent();
        BindingContext = Application.Current.Handler.MauiContext.Services.GetService<WelcomeViewModel>();

    }
    // Manejador para el evento Clicked del boton "Iniciar Sesion"
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Navega a la LoginPage.
        // Navigation es una propiedad de ContentPage cuando esta dentro de una NavigationPage.
        await Navigation.PushAsync(new LoginPage());
    }

    // Manejador para el evento Clicked del boton "Registrarse"
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // Navega a la RegisterPage.
        await Navigation.PushAsync(new RegisterPage());
    }
}
