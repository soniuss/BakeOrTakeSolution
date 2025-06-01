using Microsoft.Extensions.Logging;
using proyectoFin.MVVM.View;
using proyectoFin.MVVM.ViewModel;
using proyectoFin.Services;
using Refit;

namespace proyectoFin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            var baseApiUrl = "https://bakeortakesolution-production.up.railway.app/"; 

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseApiUrl) });
            builder.Services.AddRefitClient<IBakeOrTakeApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseApiUrl));

            // --- Registro de ViewModels ---
            // Se registran como Transient para que se cree una nueva instancia
            // cada vez que se soliciten, lo cual es común para ViewModels asociados a páginas.
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ClientMainViewModel>();
            builder.Services.AddTransient<EmpresaMainViewModel>();
            builder.Services.AddTransient<RecetaDetalleViewModel>();
            builder.Services.AddTransient<WelcomeViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<RecipesViewModel>();
            builder.Services.AddTransient<MyRecipesViewModel>();
            builder.Services.AddTransient<FavoritesViewModel>();
            builder.Services.AddTransient<EmpresaDashboardViewModel>();
            builder.Services.AddTransient<ManageRecipesViewModel>(); // Asegúrate de que este nombre de clase coincide con el archivo ManageRecipesViewModel.cs
            builder.Services.AddTransient<EmpresaProfileViewModel>(); // El ViewModel para el perfil de la empresa
            builder.Services.AddTransient<RecipeFormPage>(); 


            // --- Registro de Páginas ---
            // La mayoría se registran como Transient para obtener nuevas instancias al navegar.
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<WelcomePage>();
            builder.Services.AddTransient<ClientMainPage>();
            builder.Services.AddTransient<EmpresaMainPage>();
            builder.Services.AddTransient<RecetaDetallePage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<RecipesPage>();
            builder.Services.AddTransient<MyRecipesPage>();
            builder.Services.AddTransient<FavoritesPage>();
            builder.Services.AddTransient<MyOrdersPage>(); // Para la página de pedidos (clientes y/o empresas)
            builder.Services.AddTransient<ManageRecipesPage>(); // Para la página de gestión de recetas de empresa
            builder.Services.AddTransient<CompanyProfilePage>(); // La página de perfil de empresa
            builder.Services.AddTransient<RecipeFormPage>();


            return builder.Build();
        }
    }
}
