using proyectoFin.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection; // �Importante a�adir este using si la usas!

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
        // y asegurarte de que LoginPage tambi�n se resuelva con DI:
        // var loginPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<LoginPage>();
        // await Navigation.PushAsync(loginPage);

        // Sin embargo, si la idea es simplemente "volver" en la pila de navegaci�n,
        // PopAsync es m�s eficiente y no necesita DI para la p�gina de destino.
        // Asumo que tu intenci�n es volver a la p�gina anterior (ya sea LoginPage o WelcomePage).
        await Navigation.PopAsync();
    }
}