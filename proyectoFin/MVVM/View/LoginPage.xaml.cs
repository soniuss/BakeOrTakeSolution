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
        // Obtener la RegisterPage del contenedor de servicios 
        var registerPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }

}