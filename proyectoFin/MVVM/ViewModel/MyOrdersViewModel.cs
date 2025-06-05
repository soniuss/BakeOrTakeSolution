using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using Domain.Model; // Para PedidoOferta (si se usa en la colecci�n)
using Domain.Model.ApiResponses; // Para PedidoOfertaResponse
using Domain.Model.ApiRequests; // Para ValoracionRequest
using Microsoft.Maui.Storage;
using System;
using Refit;
using Microsoft.Extensions.DependencyInjection;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class MyOrdersViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<PedidoOfertaResponse> orders;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _userType;

        // Propiedades para el formulario de valoraci�n (si se implementa en la misma p�gina)
        [ObservableProperty]
        private int _puntuacionValoracion;
        [ObservableProperty]
        private string _comentarioValoracion;
        [ObservableProperty]
        private PedidoOfertaResponse _selectedOrderToRate; // Para saber qu� pedido se est� valorando

        public IAsyncRelayCommand LoadOrdersCommand { get; }
        public IAsyncRelayCommand<PedidoOfertaResponse> CompleteOrderCommand { get; } // Para empresa
        public IAsyncRelayCommand<PedidoOfertaResponse> ShowRateOrderFormCommand { get; } // Para cliente, mostrar formulario
        public IAsyncRelayCommand RateOrderCommand { get; } // Para cliente, enviar valoraci�n

        public MyOrdersViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            Orders = new ObservableCollection<PedidoOfertaResponse>();

            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
            CompleteOrderCommand = new AsyncRelayCommand<PedidoOfertaResponse>(CompleteOrder);
            ShowRateOrderFormCommand = new AsyncRelayCommand<PedidoOfertaResponse>(ShowRateOrderForm);
            RateOrderCommand = new AsyncRelayCommand(RateOrder);

            _ = LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            UserType = await SecureStorage.GetAsync("user_type");

            string token = await SecureStorage.GetAsync("jwt_token");
            int retryCount = 0;
            const int maxRetries = 3;
            const int retryDelayMs = 500;

            if (string.IsNullOrEmpty(token))
            {
                while (string.IsNullOrEmpty(token) && retryCount < maxRetries)
                {
                    Console.WriteLine($"DEBUG: Token no encontrado a�n en MyOrdersViewModel. Reintentando... (Intento {retryCount + 1})");
                    await Task.Delay(retryDelayMs);
                    token = await SecureStorage.GetAsync("jwt_token");
                    retryCount++;
                }
            }

            if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
            {
                ErrorMessage = "No se pudo obtener el token de autenticaci�n para cargar pedidos. Por favor, intente iniciar sesi�n de nuevo.";
                await Application.Current.MainPage.DisplayAlert("Advertencia", ErrorMessage, "OK");
                IsBusy = false;
                return;
            }

            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userIdActual))
                {
                    ErrorMessage = "No se pudo obtener el ID del usuario logueado.";
                    return;
                }

                ApiResponse<List<PedidoOfertaResponse>> response;

                if (UserType == "Cliente")
                {
                    response = await _apiService.GetClientOrdersAsync(userIdActual);
                }
                else if (UserType == "Empresa")
                {
                    response = await _apiService.GetCompanyOffersAsync(userIdActual);
                }
                else
                {
                    ErrorMessage = "Tipo de usuario desconocido para cargar pedidos.";
                    IsBusy = false;
                    return;
                }

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Orders.Clear();
                    foreach (var orderResponse in response.Content)
                    {
                        Orders.Add(orderResponse);
                    }
                    if (Orders.Count == 0)
                    {
                        ErrorMessage = "No hay pedidos/ofertas para mostrar.";
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar los pedidos. C�digo: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexi�n o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexi�n", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurri� un error inesperado al cargar los pedidos: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // M�todo para que la EMPRESA marque un pedido como completado
        private async Task CompleteOrder(PedidoOfertaResponse orderToComplete)
        {
            if (orderToComplete == null || IsBusy) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar Entrega", $"�Marcar el pedido de '{orderToComplete.RecetaNombre}' para '{orderToComplete.ClienteRealizaNombre}' como entregado?", "S�", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.CompleteOrderAsync(orderToComplete.IdPedidoOferta);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("�xito", "Pedido marcado como completado.", "OK");
                    await LoadOrdersCommand.ExecuteAsync(null); // Recargar la lista
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al completar pedido: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexi�n o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexi�n", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurri� un error inesperado al completar el pedido: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // M�todo para que el CLIENTE muestre el formulario de valoraci�n
        private async Task ShowRateOrderForm(PedidoOfertaResponse orderToRate)
        {
            if (orderToRate == null) return;

            SelectedOrderToRate = orderToRate; // Almacenar el pedido a valorar
            PuntuacionValoracion = 0; // Resetear
            ComentarioValoracion = string.Empty; // Resetear

            // Aqu� podr�as mostrar un modal o una secci�n de UI para la valoraci�n
            // Por ejemplo, usando CommunityToolkit.Maui.Views.Popup o un ContentDialog
            // Por ahora, solo mostraremos una alerta para simular el formulario.
            var result = await Application.Current.MainPage.DisplayPromptAsync(
                "Valorar Pedido",
                $"Valora tu experiencia con '{orderToRate.EmpresaNombreNegocio}' por la receta '{orderToRate.RecetaNombre}'.",
                "Enviar",
                "Cancelar",
                "Comentario (opcional)",
                maxLength: 200,
                keyboard: Keyboard.Text);

            if (result != null) // Si el usuario no cancel�
            {
                // Intentar parsear la puntuaci�n (esto es una simplificaci�n)
                var puntuacionString = await Application.Current.MainPage.DisplayPromptAsync(
                    "Puntuaci�n (1-5)",
                    "Introduce tu puntuaci�n (1-5):",
                    "Aceptar",
                    "Cancelar",
                    keyboard: Keyboard.Numeric);

                if (int.TryParse(puntuacionString, out int puntuacion))
                {
                    PuntuacionValoracion = puntuacion;
                    ComentarioValoracion = result; // El resultado del primer prompt es el comentario

                    await RateOrder(); // Llamar al comando de valoraci�n
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Puntuaci�n inv�lida. Debe ser un n�mero entre 1 y 5.", "OK");
                }
            }
        }

        // M�todo para que el CLIENTE env�e la valoraci�n
        private async Task RateOrder()
        {
            if (SelectedOrderToRate == null || IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            if (PuntuacionValoracion < 1 || PuntuacionValoracion > 5)
            {
                ErrorMessage = "La puntuaci�n debe estar entre 1 y 5.";
                IsBusy = false;
                return;
            }

            try
            {
                var request = new ValoracionRequest
                {
                    Puntuacion = PuntuacionValoracion,
                    Comentario = ComentarioValoracion
                };

                var response = await _apiService.RateOrderAsync(SelectedOrderToRate.IdPedidoOferta, request);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("�xito", "Pedido valorado exitosamente.", "OK");
                    await LoadOrdersCommand.ExecuteAsync(null); // Recargar la lista
                    SelectedOrderToRate = null; // Limpiar el pedido seleccionado
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al valorar pedido: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexi�n o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexi�n", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurri� un error inesperado al valorar el pedido: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
