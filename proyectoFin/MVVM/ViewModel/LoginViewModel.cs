﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Model.ApiRequests;
using proyectoFin.MVVM.View;
using proyectoFin.Services;
using Refit;

namespace proyectoFin.MVVM.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isBusy; // Para indicar si hay una operacion en curso

        [ObservableProperty]
        private string _errorMessage; // Para mostrar mensajes de error

        public LoginViewModel(IBakeOrTakeApi apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task PerformLogin()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new LoginRequest { Email = Email, Password = Password };
                var response = await _apiService.LoginAsync(request);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    // Login exitoso
                    var loginResult = response.Content;
                    Console.WriteLine($"Login exitoso. Token: {loginResult.Token}, Tipo: {loginResult.UserType}, ID: {loginResult.UserId}");

                    // ** GUARDAR TOKEN Y DATOS DE USUARIO EN SECURESTORAGE **
                    await SecureStorage.SetAsync("jwt_token", loginResult.Token);
                    await SecureStorage.SetAsync("user_type", loginResult.UserType);
                    await SecureStorage.SetAsync("user_id", loginResult.UserId.ToString());

                    // Navegar a la pantalla principal del cliente o empresa
                    // Reemplazamos la MainPage de la aplicacion para evitar volver al login con el boton atras
                    if (loginResult.UserType == "Cliente")
                    {
                        var clientMainPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<ClientTabsPage>();
                        Application.Current.MainPage = new NavigationPage(clientMainPage);
                    }
                    else if (loginResult.UserType == "Empresa")
                    {
                        var empresaMainPage = Application.Current.Handler.MauiContext.Services.GetRequiredService<EmpresaTabsPage>();
                        Application.Current.MainPage = new NavigationPage(empresaMainPage); // Necesitas crear esta pagina
                    }
                    else
                    {
                        ErrorMessage = "Tipo de usuario desconocido.";
                    }
                }
                else
                {
                    // Login fallido
                    ErrorMessage = $"Error al iniciar sesion: {response.ReasonPhrase}";
                    if (response.Error != null && !string.IsNullOrEmpty(response.Error.Content))
                    {
                        Console.WriteLine($"Detalles del error API: {response.Error.Content}");
                    }
                }
            }
            catch (ApiException ex) // Errores de red o de la API (ej. 404, 500)
            {
                ErrorMessage = $"Error de red o API: {ex.Message}";
                Console.WriteLine($"API Exception: {ex}");
            }
            catch (Exception ex) // Otros errores inesperados
            {
                ErrorMessage = $"Ocurrió un error inesperado: {ex.Message}";
                Console.WriteLine($"General Exception: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
