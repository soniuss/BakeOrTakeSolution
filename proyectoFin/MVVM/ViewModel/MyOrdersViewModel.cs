using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using proyectoFin.Services;
using Domain.Model; // Para PedidoOferta (si se usa en la colección)
using Domain.Model.ApiResponses; // Para PedidoOfertaResponse
using Microsoft.Maui.Storage;
using System;
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
        private string _userType;

        public IAsyncRelayCommand LoadOrdersCommand { get; }

        public MyOrdersViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            Orders = new ObservableCollection<PedidoOfertaResponse>();

            LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);

            _ = LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            UserType = await SecureStorage.GetAsync("user_type"); // Obtener el tipo de usuario

            // Lógica de espera/reintento para el token
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

                if (UserType == "Cliente")
                {
                    response = await _apiService.GetClientOrdersAsync(userIdActual);
                }
                else if (UserType == "Empresa")
                {
                    response = await _apiService.GetCompanyOffersAsync(userIdActual); // ¡CORRECTO! Llama al endpoint de empresa
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
                        ErrorMessage = "No hay pedidos/ofertas para mostrar."; // Mensaje si está vacío
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
    }
}
