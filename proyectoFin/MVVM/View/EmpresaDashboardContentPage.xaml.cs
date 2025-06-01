using proyectoFin.MVVM.ViewModel;

namespace proyectoFin.MVVM.View
{
    public partial class EmpresaDashboardContentPage : ContentPage
    {
        public EmpresaDashboardContentPage(EmpresaDashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}