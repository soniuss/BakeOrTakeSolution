using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // ¡Importante añadir este using si la usas!

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
        // Si quieres ir directamente a LoginPage en lugar de solo PopAsync
        // y asegurarte de que LoginPage también se resuelva con DI:
        // var loginPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<LoginPage>();
        // await Navigation.PushAsync(loginPage);

        // Sin embargo, si la idea es simplemente "volver" en la pila de navegación,
        // PopAsync es más eficiente y no necesita DI para la página de destino.
        // Asumo que tu intención es volver a la página anterior (ya sea LoginPage o WelcomePage).
        await Navigation.PopAsync();
    }
}