using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // ¡Importante añadir este using!

namespace proyectoFin.MVVM.View;

public partial class WelcomePage : ContentPage
{
    // Inyecta el ViewModel en el constructor
    public WelcomePage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Asigna el ViewModel inyectado
    }

    // Manejador para el evento Clicked del boton "Iniciar Sesion"
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // *** CAMBIO CRÍTICO AQUÍ: Obtener la LoginPage del contenedor de servicios ***
        var loginPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<LoginPage>();
        await Navigation.PushAsync(loginPage);
    }

    // Manejador para el evento Clicked del boton "Registrarse"
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // *** CAMBIO CRÍTICO AQUÍ: Obtener la RegisterPage del contenedor de servicios ***
        var registerPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }
}