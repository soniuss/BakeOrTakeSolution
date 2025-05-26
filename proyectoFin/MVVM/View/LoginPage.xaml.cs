using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // ¡Importante añadir este using si la usas!

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
        // *** CAMBIO CRÍTICO AQUÍ: Obtener la RegisterPage del contenedor de servicios ***
        var registerPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }

    // Si tienes un botón de "Volver" o "Cancelar" en LoginPage, y no usas el botón de navegación predeterminado
    // y quieres volver a la página anterior (WelcomePage), PopAsync es correcto.
    // Si quisieras navegar a WelcomePage directamente usando DI:
    // private async void OnBackToWelcomeClicked(object sender, EventArgs e)
    // {
    //     var welcomePage = Application.Current.Handler.MauiContext.Services.GetRequiredService<WelcomePage>();
    //     await Navigation.PushAsync(welcomePage);
    // }
}