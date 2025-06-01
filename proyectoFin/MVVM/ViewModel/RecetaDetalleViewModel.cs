using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model;
using proyectoFin.Services;
using Refit;
using System.Threading.Tasks;
using System; // Para Exception

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecetaDetalleViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

        [ObservableProperty]
        private Receta _receta;

        [ObservableProperty]
        private int _recetaId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        // Propiedad de comando público para cargar la receta
        public IAsyncRelayCommand LoadRecetaCommand { get; }

        // Comando para hacer una oferta (sin implementar aún)
        public IAsyncRelayCommand MakeOfferCommand { get; }

        public RecetaDetalleViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            LoadRecetaCommand = new AsyncRelayCommand(LoadReceta); // Inicializa el comando
            MakeOfferCommand = new AsyncRelayCommand(MakeOffer); // Inicializa el comando
        }

        // Método privado que es ejecutado por LoadRecetaCommand
        private async Task LoadReceta()
        {
            if (IsBusy || RecetaId == 0) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.GetRecetaByIdAsync(RecetaId);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Receta = response.Content;
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al cargar receta: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al cargar receta: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al cargar receta: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar la receta: {ex.Message}";
                Console.WriteLine($"General Exception en LoadReceta: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Método para hacer una oferta (aún sin implementar lógica)
        private async Task MakeOffer()
        {
            // Lógica para crear una oferta
            await Application.Current.MainPage.DisplayAlert("Oferta", "Funcionalidad de hacer oferta en desarrollo.", "OK");
        }
    }
}
