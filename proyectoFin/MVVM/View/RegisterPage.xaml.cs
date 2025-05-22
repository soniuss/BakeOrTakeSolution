using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
        BindingContext = Application.Current.Handler.MauiContext.Services.GetService<RegisterViewModel>();
    }
    // Manejador para el evento Clicked del boton "Inicia sesion aqui"
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Vuelve a la pagina anterior (Login o Welcome)
        await Navigation.PopAsync(); // Esto asume que vienes de LoginPage o WelcomePage
                                     // Si quieres ir directamente a LoginPage, podrias hacer:
                                     // await Navigation.PushAsync(new LoginPage());
    }
}
