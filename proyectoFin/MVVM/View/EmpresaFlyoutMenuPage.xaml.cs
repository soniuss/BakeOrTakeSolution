namespace proyectoFin.MVVM.View
{
    public partial class EmpresaFlyoutMenuPage : ContentPage
    {
        public EmpresaFlyoutMenuPage()
        {
            InitializeComponent();
            // El BindingContext de esta p�gina ser� el de su padre (EmpresaMainPage, la FlyoutPage)
            // que es el EmpresaMainViewModel.
        }
    }
}