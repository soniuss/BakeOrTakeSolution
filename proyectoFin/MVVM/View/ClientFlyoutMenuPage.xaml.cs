namespace proyectoFin.MVVM.View
{
    public partial class ClientFlyoutMenuPage : ContentPage
    {
        public ClientFlyoutMenuPage()
        {
            InitializeComponent();
            // El BindingContext de esta p�gina ser� el de su padre (ClientMainPage, la FlyoutPage)
            // que es el ClientMainViewModel. Por eso los Commands se enlazan directamente.
        }
    }
}