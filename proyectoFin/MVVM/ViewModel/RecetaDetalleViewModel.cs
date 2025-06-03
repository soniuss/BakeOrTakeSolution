using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model.ApiResponses; // ¡NUEVO! Para RecetaResponse
using proyectoFin.Services;
using Refit;
using System.Threading.Tasks;
using System; // Para Exception
using Microsoft.Maui.Controls; // Para Application.Current.MainPage.DisplayAlert

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecetaDetalleViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

        // ¡CORRECCIÓN CLAVE AQUÍ! Cambiar el tipo de la propiedad a RecetaResponse
        [ObservableProperty]
        private RecetaResponse _receta;

        [ObservableProperty]
        private int _recetaId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public IAsyncRelayCommand LoadRecetaCommand { get; }
        public IAsyncRelayCommand MakeOfferCommand { get; }

        public RecetaDetalleViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            LoadRecetaCommand = new AsyncRelayCommand(LoadReceta);
            MakeOfferCommand = new AsyncRelayCommand(MakeOffer);
        }

        private async Task LoadReceta()
        {
            if (IsBusy || RecetaId == 0) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // `GetRecetaByIdAsync` ya devuelve `ApiResponse<RecetaResponse>`
                var response = await _apiService.GetRecetaByIdAsync(RecetaId);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Receta = response.Content; // ¡Ahora los tipos coinciden!
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

        private async Task MakeOffer()
        {
            // Lógica para crear una oferta
            await Application.Current.MainPage.DisplayAlert("Oferta", "Funcionalidad de hacer oferta en desarrollo.", "OK");
        }
    }
}