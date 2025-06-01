using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services; // Para tu API
using Domain.Model; // Para tu clase PedidoOferta
using Microsoft.Maui.Storage; // Para SecureStorage
using System; // Para Exception, Console.WriteLine

namespace proyectoFin.MVVM.ViewModel
{
    public partial class MyOrdersViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

        // --- Propiedades Observables (para solucionar CS0103 IsBusy) ---
        [ObservableProperty]
        private ObservableCollection<PedidoOferta> orders; // La colección de pedidos

        [ObservableProperty]
        private bool _isBusy; // Propiedad para el indicador de actividad

        [ObservableProperty]
        private string _errorMessage; // Propiedad para mensajes de error

        // Propiedad calculada para el estado de "no ocupado"
        public bool IsNotBusy => !IsBusy;


        // --- Comandos ---
        public IAsyncRelayCommand LoadOrdersCommand { get; }
        // Si necesitas un comando para seleccionar un pedido, añádelo aquí:
        // public IRelayCommand<PedidoOferta> SelectOrderCommand { get; }


        public MyOrdersViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
            Orders = new ObservableCollection<PedidoOferta>(); // Inicializa la colección

            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
            // SelectOrderCommand = new RelayCommand<PedidoOferta>(OnOrderSelected); // Si lo añades

            // Inicia la carga de pedidos cuando el ViewModel se crea
            _ = LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            if (IsBusy) return; // Evitar llamadas múltiples

            IsBusy = true;       // Activa el indicador de actividad
            ErrorMessage = string.Empty; // Limpia mensajes de error anteriores

            try
            {
                // Obtener el ID del cliente logueado desde SecureStorage
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int idClienteActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado.";
                    return;
                }

                // --- Solución para CS1061: Llamada al nuevo método de API ---
                // Necesitas definir GetClientOrdersAsync en tu IBakeOrTakeApi
                // y su implementación en el controlador de la API REST.
                var response = await _apiService.GetClientOrdersAsync(idClienteActual);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Orders.Clear(); // Limpia la colección antes de añadir los nuevos pedidos
                    foreach (var order in response.Content)
                    {
                        Orders.Add(order);
                    }
                }
                else
                {
                    // Manejo de errores de la API
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar los pedidos. Código: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                // Manejo de errores de Refit (problemas de conexión, errores HTTP del servidor)
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                // Otros errores inesperados
                ErrorMessage = $"Ocurrió un error inesperado al cargar los pedidos: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false; // Desactiva el indicador de actividad al finalizar
            }
        }

        // Si añades un comando para seleccionar un pedido, aquí estaría su lógica:
        // private void OnOrderSelected(PedidoOferta selectedOrder)
        // {
        //     if (selectedOrder != null)
        //     {
        //         Console.WriteLine($"Pedido seleccionado: {selectedOrder.id_pedido_oferta}");
        //         // Aquí podrías navegar a una página de detalles del pedido
        //     }
        // }
    }
}