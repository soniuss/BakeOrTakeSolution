
using CommunityToolkit.Mvvm.ComponentModel; // <--- ¡Asegúrate de tener este!
using CommunityToolkit.Mvvm.Input;       // <--- ¡Asegúrate de tener este!
using Domain.Model;
using proyectoFin.Services;
using Refit;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecetaDetalleViewModel : ObservableObject // <--- ¡Debe ser partial!
    {
        private readonly IBakeOrTakeApi _apiService;

        [ObservableProperty]
        private Receta _receta;

        [ObservableProperty] // <--- ¡Asegúrate de que esté aquí!
        private int _recetaId;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        public RecetaDetalleViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand] // <--- ¡Asegúrate de que esté aquí!
        private async Task LoadReceta()
        {
            // ... (tu lógica para cargar la receta)
        }

        [RelayCommand]
        private async Task MakeOffer()
        {
            // ...
        }
    }
}