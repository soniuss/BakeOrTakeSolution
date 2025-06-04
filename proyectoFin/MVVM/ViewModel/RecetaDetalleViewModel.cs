using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model.ApiResponses; // Para RecetaResponse, PedidoOfertaResponse
using Domain.Model.ApiRequests; // Para OfertaRequest, PedidoRequest
using proyectoFin.Services;
using Refit;
using System.Threading.Tasks;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using proyectoFin.MVVM.View; // ¡CORRECCIÓN CLAVE AQUÍ! Para ObservableCollection

namespace proyectoFin.MVVM.ViewModel
{
    public partial class RecetaDetalleViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

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

        [ObservableProperty]
        private string _userType;
        public bool IsClientUser => UserType == "Cliente";
        public bool IsCompanyUser => UserType == "Empresa";

        [ObservableProperty]
        private bool _isRecipeOwnedByUser;

        [ObservableProperty]
        private ObservableCollection<PedidoOfertaResponse> _offers;
        public bool HasOffers => Offers.Count > 0;

        [ObservableProperty]
        private double _offerPrice;
        [ObservableProperty]
        private string _offerDescription;
        [ObservableProperty]
        private bool _offerAvailability = true;

        // Comandos
        public IAsyncRelayCommand LoadRecetaCommand { get; }
        public IAsyncRelayCommand MakeOfferCommand { get; }
        public IAsyncRelayCommand<PedidoOfertaResponse> PlaceOrderCommand { get; }
        public IAsyncRelayCommand EditRecipeCommand { get; }
        public IAsyncRelayCommand LoadOffersCommand { get; }

        public RecetaDetalleViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            Offers = new ObservableCollection<PedidoOfertaResponse>();
            Offers.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasOffers));

            LoadRecetaCommand = new AsyncRelayCommand(LoadReceta);
            MakeOfferCommand = new AsyncRelayCommand(MakeOffer);
            PlaceOrderCommand = new AsyncRelayCommand<PedidoOfertaResponse>(PlaceOrder);
            EditRecipeCommand = new AsyncRelayCommand(EditRecipe);
            LoadOffersCommand = new AsyncRelayCommand(LoadOffers);
        }

        private async Task LoadReceta()
        {
            if (IsBusy || RecetaId == 0) return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            IsRecipeOwnedByUser = false;
            UserType = await SecureStorage.GetAsync("user_type");

            try
            {
                var response = await _apiService.GetRecetaByIdAsync(RecetaId);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Receta = response.Content;

                    var userIdString = await SecureStorage.GetAsync("user_id");
                    if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int loggedInUserId))
                    {
                        IsRecipeOwnedByUser = (Receta.IdClienteCreador == loggedInUserId);
                    }

                    await LoadOffersCommand.ExecuteAsync(null);
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

        private async Task LoadOffers()
        {
            if (RecetaId == 0) return;

            try
            {
                var response = await _apiService.GetOffersByRecetaAsync(RecetaId);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    Offers.Clear();
                    foreach (var offer in response.Content)
                    {
                        Offers.Add(offer);
                    }
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    Console.WriteLine($"API Error al cargar ofertas: {response.StatusCode}. Detalles: {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                Console.WriteLine($"Refit API Exception al cargar ofertas: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception en LoadOffers: {ex}");
            }
        }

        private async Task MakeOffer()
        {
            if (IsBusy || Receta == null || !IsCompanyUser) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            if (OfferPrice <= 0 || string.IsNullOrWhiteSpace(OfferDescription))
            {
                ErrorMessage = "El precio debe ser mayor que 0 y la descripción de la oferta es obligatoria.";
                IsBusy = false;
                return;
            }

            try
            {
                var request = new OfertaRequest
                {
                    Precio = OfferPrice,
                    Disponibilidad = OfferAvailability,
                    DescripcionOferta = OfferDescription
                };

                var response = await _apiService.CreateOfferAsync(Receta.IdReceta, request);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Oferta creada exitosamente.", "OK");
                    await LoadOffersCommand.ExecuteAsync(null);
                    OfferPrice = 0;
                    OfferDescription = string.Empty;
                    OfferAvailability = true;
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al crear oferta: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al crear oferta: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al crear oferta: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al crear la oferta: {ex.Message}";
                Console.WriteLine($"General Exception en MakeOffer: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PlaceOrder(PedidoOfertaResponse selectedOffer)
        {
            if (IsBusy || Receta == null || !IsClientUser || selectedOffer == null) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar Pedido", $"¿Confirmas el pedido de '{Receta.Nombre}' a '{selectedOffer.EmpresaNombreNegocio}' por {selectedOffer.Precio:C}?", "Sí", "No");
                if (!confirm)
                {
                    IsBusy = false;
                    return;
                }

                var request = new PedidoRequest
                {
                    IdPedidoOferta = selectedOffer.IdPedidoOferta
                };

                var response = await _apiService.PlaceOrderAsync(request);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Pedido realizado exitosamente.", "OK");
                    await LoadOffersCommand.ExecuteAsync(null);
                }
                else
                {
                    var errorContent = response.Error?.Content;
                    ErrorMessage = $"Error al realizar pedido: {response.StatusCode}. Detalles: {errorContent}";
                    Console.WriteLine($"API Error al realizar pedido: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Error de conexión o API: {(int)ex.StatusCode} - {ex.Message}. Detalles: {ex.Content}";
                Console.WriteLine($"Refit API Exception al realizar pedido: {(int)ex.StatusCode} - {ex.Message} - {ex.Content}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ocurrió un error inesperado al realizar el pedido: {ex.Message}";
                Console.WriteLine($"General Exception en PlaceOrder: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task EditRecipe()
        {
            if (Receta == null) return;

            if (Application.Current.MainPage is NavigationPage mainNavPage && mainNavPage.CurrentPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage currentTabPageNav)
            {
                var recipeFormPage = _serviceProvider.GetService<RecipeFormPage>();
                if (recipeFormPage != null)
                {
                    if (recipeFormPage.BindingContext is RecipeFormViewModel formViewModel)
                    {
                        await formViewModel.InitializeAsync(Receta.IdReceta);
                    }
                    await currentTabPageNav.PushAsync(recipeFormPage);
                }
                else
                {
                    Console.WriteLine("Error: RecipeFormPage no pudo ser resuelta para edición desde RecetaDetalle.");
                    await Application.Current.MainPage.DisplayAlert("Error de navegación", "No se pudo cargar la página para editar recetas.", "OK");
                }
            }
            else
            {
                Console.WriteLine("Error: Contexto de navegación inesperado para Editar Receta desde RecetaDetalle.");
                await Application.Current.MainPage.DisplayAlert("Error", "Contexto de navegación no compatible.", "OK");
            }
        }
    }
}
