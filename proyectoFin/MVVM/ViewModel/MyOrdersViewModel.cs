using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using proyectoFin.Services;
using Domain.Model.ApiResponses;
using Domain.Model.ApiRequests; 
using Refit;

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
        [NotifyPropertyChangedFor(nameof(IsClientUser))]
        [NotifyPropertyChangedFor(nameof(IsCompanyUser))]
        private string _userType;

        public bool IsClientUser => UserType == "Cliente";
        public bool IsCompanyUser => UserType == "Empresa";

        // Propiedades para el formulario de valoración
        [ObservableProperty]
        private int _puntuacionValoracion;
        [ObservableProperty]
        private string _comentarioValoracion;
        private PedidoOfertaResponse _selectedOrderToRate;

        public IAsyncRelayCommand LoadOrdersCommand { get; }
        public IAsyncRelayCommand<PedidoOfertaResponse> CompleteOrderCommand { get; }
        public IAsyncRelayCommand<PedidoOfertaResponse> ShowRateOrderFormCommand { get; }
        public IAsyncRelayCommand RateOrderCommand { get; }

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
                    Console.WriteLine($"DEBUG: Token no encontrado aún en MyOrdersViewModel. Reintentando... (Intento {retryCount + 1})");
                    await Task.Delay(retryDelayMs);
                    token = await SecureStorage.GetAsync("jwt_token");
                    retryCount++;
                }
            }

            if (string.IsNullOrEmpty(token) && maxRetries > 0 && retryCount == maxRetries)
            {
                ErrorMessage = "No se pudo obtener el token de autenticación para cargar pedidos. Por favor, intente iniciar sesión de nuevo.";
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
                List<PedidoOfertaResponse> filteredContent = new List<PedidoOfertaResponse>(); // Nuevo

                if (UserType == "Cliente")
                {
                    response = await _apiService.GetClientOrdersAsync(userIdActual);
                    filteredContent = response.Content; // Para clientes, no hay filtro inicial
                }
                else if (UserType == "Empresa")
                {
                    response = await _apiService.GetCompanyOffersAsync(userIdActual);
                   
                    filteredContent = response.Content?.Where(po => po.IdClienteRealiza.HasValue).ToList();
                }
                else
                {
                    ErrorMessage = "Tipo de usuario desconocido para cargar pedidos.";
                    IsBusy = false;
                    return;
                }

                if (response.IsSuccessStatusCode && filteredContent != null) // Usar filteredContent
                {
                    Orders.Clear();
                    foreach (var orderResponse in filteredContent) // Usar filteredContent
                    {
                        Orders.Add(orderResponse);
                    }
                    if (Orders.Count == 0)
                    {
                        ErrorMessage = "No hay pedidos/ofertas para mostrar.";
                    }
                    else
                    {
                        ErrorMessage = string.Empty;
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"No se pudieron cargar los pedidos. Código: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error de Carga", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al cargar los pedidos: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CompleteOrder(PedidoOfertaResponse orderToComplete)
        {
            if (orderToComplete == null || IsBusy || UserType != "Empresa") return;

            if (orderToComplete.Estado != "Pedido_Pendiente")
            {
                await Application.Current.MainPage.DisplayAlert("Advertencia", "Este pedido no está pendiente o ya fue entregado.", "OK");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar Entrega", $"¿Marcar el pedido de '{orderToComplete.RecetaNombre}' para '{orderToComplete.ClienteRealizaNombre}' como entregado?", "Sí", "No");
            if (!confirm) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.CompleteOrderAsync(orderToComplete.IdPedidoOferta);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Pedido marcado como entregado.", "OK");
                    await LoadOrdersCommand.ExecuteAsync(null);
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al marcar como entregado: {response.StatusCode}. Detalles: {errorContent}";
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al marcar como entregado: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowRateOrderForm(PedidoOfertaResponse orderToRate)
        {
            if (orderToRate == null || IsBusy || UserType != "Cliente") return;

            if (orderToRate.Estado != "Pedido_Completado" || orderToRate.Puntuacion.HasValue)
            {
                await Application.Current.MainPage.DisplayAlert("Advertencia", "Este pedido no está listo para ser valorado o ya ha sido valorado.", "OK");
                return;
            }

            _selectedOrderToRate = orderToRate;
            PuntuacionValoracion = 0;
            ComentarioValoracion = string.Empty;

            string puntuacionStr = await Application.Current.MainPage.DisplayPromptAsync("Valorar Pedido", "Introduce tu puntuación (1-5):", "OK", "Cancelar", placeholder: "Ej: 5", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(puntuacionStr) || !int.TryParse(puntuacionStr, out int puntuacion) || puntuacion < 1 || puntuacion > 5)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Puntuación inválida. Debe ser un número entre 1 y 5.", "OK");
                return;
            }

            string comentario = await Application.Current.MainPage.DisplayPromptAsync("Valorar Pedido", "Deja un comentario (opcional):", "OK", "Cancelar", placeholder: "Ej: Muy rico!");

            PuntuacionValoracion = puntuacion;
            ComentarioValoracion = comentario;

            await RateOrder();
        }

        private async Task RateOrder()
        {
            if (_selectedOrderToRate == null || IsBusy || UserType != "Cliente") return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            if (PuntuacionValoracion < 1 || PuntuacionValoracion > 5)
            {
                ErrorMessage = "La puntuación debe estar entre 1 y 5.";
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

                var response = await _apiService.RateOrderAsync(_selectedOrderToRate.IdPedidoOferta, request);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Pedido valorado exitosamente.", "OK");
                    await LoadOrdersCommand.ExecuteAsync(null);
                    _selectedOrderToRate = null;
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
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al valorar el pedido: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
