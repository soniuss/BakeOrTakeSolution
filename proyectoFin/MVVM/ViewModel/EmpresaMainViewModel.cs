using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using proyectoFin.Services;
using proyectoFin.MVVM.View;
using Domain.Model; // ¡IMPORTANTE! Para tus clases de dominio si las usas aquí

namespace proyectoFin.MVVM.ViewModel
{
    public partial class EmpresaMainViewModel : ObservableObject
    {
        private readonly IBakeOrTakeApi _apiService;
        private readonly IServiceProvider _serviceProvider;

        public IRelayCommand NavigateToMyOrdersCommand { get; }
        public IRelayCommand NavigateToManageRecipesCommand { get; }
        public IRelayCommand NavigateToCompanyProfileCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public EmpresaMainViewModel(IBakeOrTakeApi apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            NavigateToMyOrdersCommand = new RelayCommand(async () => await NavigateToPage<MyOrdersPage>()); // Asume MyOrdersPage
            NavigateToManageRecipesCommand = new RelayCommand(async () => await NavigateToPage<ManageRecipesPage>()); // Asume ManageRecipesPage
            NavigateToCompanyProfileCommand = new RelayCommand(async () => await NavigateToPage<CompanyProfilePage>()); // Asume CompanyProfilePage
            LogoutCommand = new RelayCommand(async () => await PerformLogout());
        }

        private async Task NavigateToPage<TPage>() where TPage : Page
        {
            var page = _serviceProvider.GetService<TPage>();

            if (page != null)
            {
                if (Application.Current.MainPage is FlyoutPage flyoutPage)
                {
                    if (flyoutPage.Detail is NavigationPage navigationPage)
                    {
                        await navigationPage.PopToRootAsync();
                        await navigationPage.PushAsync(page);
                        flyoutPage.IsPresented = false;
                    }
                    else
                    {
                        flyoutPage.Detail = new NavigationPage(page);
                        flyoutPage.IsPresented = false;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: La página {typeof(TPage).Name} no pudo ser resuelta.");
                await Application.Current.MainPage.DisplayAlert("Error de navegación", $"No se pudo cargar la página {typeof(TPage).Name}.", "OK");
            }
        }

        private async Task PerformLogout()
        {
            await Application.Current.MainPage.DisplayAlert("Sesión Finalizada", "Has cerrado sesión como Empresa.", "OK");
            var loginPage = _serviceProvider.GetService<LoginPage>();
            if (loginPage != null)
            {
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
        }
    }
}