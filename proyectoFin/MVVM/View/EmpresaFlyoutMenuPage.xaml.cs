namespace proyectoFin.MVVM.View
{
    public partial class EmpresaFlyoutMenuPage : ContentPage
    {
        public EmpresaFlyoutMenuPage()
        {
            InitializeComponent();
            // El BindingContext de esta página será el de su padre (EmpresaMainPage, la FlyoutPage)
            // que es el EmpresaMainViewModel.
        }
    }
}