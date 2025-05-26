using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // �Importante a�adir este using si la usas!

namespace proyectoFin.MVVM.View;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel) // Inyecta LoginViewModel
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Manejador para el evento Clicked del boton "Registrate aqui"
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // *** CAMBIO CR�TICO AQU�: Obtener la RegisterPage del contenedor de servicios ***
        var registerPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }

    // Si tienes un bot�n de "Volver" o "Cancelar" en LoginPage, y no usas el bot�n de navegaci�n predeterminado
    // y quieres volver a la p�gina anterior (WelcomePage), PopAsync es correcto.
    // Si quisieras navegar a WelcomePage directamente usando DI:
    // private async void OnBackToWelcomeClicked(object sender, EventArgs e)
    // {
    //     var welcomePage = Application.Current.Handler.MauiContext.Services.GetRequiredService<WelcomePage>();
    //     await Navigation.PushAsync(welcomePage);
    // }
}