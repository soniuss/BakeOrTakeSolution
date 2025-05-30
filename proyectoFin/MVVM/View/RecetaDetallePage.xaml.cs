using Microsoft.Maui.Controls;
using proyectoFin.MVVM.ViewModel; // Asegúrate de tener este using

namespace proyectoFin.MVVM.View
{
    public partial class RecetaDetallePage : ContentPage
    {
        // Constructor por defecto, o que reciba el ViewModel (si usas DI fuerte aquí)
        public RecetaDetallePage(RecetaDetalleViewModel viewModel) // Inyecta el ViewModel
        {
            InitializeComponent();
            BindingContext = viewModel; // Asigna el ViewModel inyectado al BindingContext
        }

        // Si prefieres que el BindingContext se asigne en el XAML, puedes tener un constructor sin parámetros:
        // public RecetaDetallePage()
        // {
        //     InitializeComponent();
        // }
    }
}