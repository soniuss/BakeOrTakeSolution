using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model;
using proyectoFin.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System;
using proyectoFin.MVVM.View;
using Refit;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class ClientMainViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<Receta> _recetas;

        [ObservableProperty]
        private Receta _selectedReceta;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage;

        public ClientMainViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            Recetas = new ObservableCollection<Receta>();

            // Llamar al método directamente
            _ = LoadRecetas();
        }

        private async Task LoadRecetas()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.GetRecetasAsync();

                if (response.IsSuccessStatusCode)
                {
                    Recetas.Clear();
                    foreach (var receta in response.Content)
                    {
                        Recetas.Add(receta);
                    }
                }
                else
                {
                    ErrorMessage = "No se pudieron cargar las recetas desde el servidor.";
                    Console.WriteLine($"API Error: {response.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar recetas: {ex.Message}");
                ErrorMessage = "Ocurrió un error al intentar cargar las recetas.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SelectReceta()
        {
            if (SelectedReceta != null)
            {
                Console.WriteLine($"Receta seleccionada: {SelectedReceta.nombre} (ID: {SelectedReceta.id_receta})");

                if (Application.Current.MainPage is NavigationPage navPage)
                {
                    var recetaDetallePage = _serviceProvider.GetService<RecetaDetallePage>();

                    if (recetaDetallePage != null && recetaDetallePage.BindingContext is RecetaDetalleViewModel viewModel)
                    {
                        viewModel.RecetaId = SelectedReceta.id_receta;
                        await viewModel.LoadRecetaCommand.ExecuteAsync(null);
                        await navPage.PushAsync(recetaDetallePage);
                    }
                    else
                    {
                        Console.WriteLine("Error: No se pudo obtener RecetaDetallePage o su ViewModel.");
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar la página de detalle de la receta.", "OK");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Application.Current.MainPage no es una NavigationPage.");
                    await Application.Current.MainPage.DisplayAlert("Error", "La página principal no es una NavigationPage.", "OK");
                }

                SelectedReceta = null;
            }
        }

        [RelayCommand]
        private async Task CerrarSesion()
        {
            try
            {
                // Usa directamente la clase SecureStorage
                await SecureStorage.SetAsync("token", null);
                await SecureStorage.SetAsync("usuarioId", null);
                await SecureStorage.SetAsync("rol", null);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error limpiando SecureStorage: {ex.Message}");
            }

            var welcomePage = _serviceProvider.GetService<WelcomePage>();
            if (welcomePage != null)
            {
                Application.Current.MainPage = new NavigationPage(welcomePage);
            }
            else
            {
                Console.WriteLine("Error: No se pudo obtener WelcomePage del ServiceProvider.");
                Application.Current.MainPage = new NavigationPage(
                    new WelcomePage(
                        new WelcomeViewModel(
                            _serviceProvider.GetService<IBakeOrTakeApi>(),
                            _serviceProvider
                        )
                    )
                );
            }
        }

    }
}
