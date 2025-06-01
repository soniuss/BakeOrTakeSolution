using proyectoFin.MVVM.ViewModel; // Asegúrate de tener este using

namespace proyectoFin.MVVM.View
{
    public partial class MyRecipesPage : ContentPage
    {
        // Constructor para inyección de dependencia
        public MyRecipesPage(MyRecipesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}