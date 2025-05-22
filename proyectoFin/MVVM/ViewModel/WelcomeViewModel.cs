using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class WelcomeViewModel : ObservableObject
    {
        public WelcomeViewModel()
        {
            // Este ViewModel no necesita servicios inyectados por ahora.
        }

        // Comando para ir a la pagina de inicio de sesion.
        // La navegacion real se gestionara en el code-behind de WelcomePage.xaml.cs
        [RelayCommand]
        private async Task GoToLoginPage()
        {
            // Solo para que el comando sea async y no de warning.
            // La logica de navegacion (PushAsync) se hara en la Vista.
            await Task.CompletedTask;
        }

        // Comando para ir a la pagina de registro.
        [RelayCommand]
        private async Task GoToRegisterPage()
        {
            // Similar a GoToLoginPage.
            await Task.CompletedTask;
        }
    }
}
