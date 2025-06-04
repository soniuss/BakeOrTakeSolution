using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using proyectoFin.Converters;
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
            builder.Services.AddTransient<AuthHeaderHandler>();

            builder.Services.AddRefitClient<IBakeOrTakeApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseApiUrl))
                .AddHttpMessageHandler<AuthHeaderHandler>();

            // --- Registro de ViewModels ---
            // Se registran como Transient para que se cree una nueva instancia
            // cada vez que se soliciten, lo cual es común para ViewModels asociados a páginas.

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<RecetaDetalleViewModel>();
            builder.Services.AddTransient<WelcomeViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<RecipesViewModel>();
            builder.Services.AddTransient<MyRecipesViewModel>();
            builder.Services.AddTransient<FavoritesViewModel>();
            builder.Services.AddTransient<EmpresaProfileViewModel>();
            builder.Services.AddTransient<RecipeFormViewModel>();
            builder.Services.AddTransient<MyOrdersViewModel>();

            // --- Registro de Páginas ---
            // La mayoría se registran como Transient para obtener nuevas instancias al navegar.
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<WelcomePage>();
            builder.Services.AddTransient<RecetaDetallePage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<RecipesPage>();
            builder.Services.AddTransient<MyRecipesPage>();
            builder.Services.AddTransient<FavoritesPage>();
            builder.Services.AddTransient<MyOrdersPage>();
            builder.Services.AddTransient<CompanyProfilePage>();
            builder.Services.AddTransient<RecipeFormPage>();
            
            // Registrar las nuevas TabbedPages
            builder.Services.AddTransient<ClientTabsPage>();
            builder.Services.AddTransient<EmpresaTabsPage>();

            // Registrar Converters
            builder.Services.AddSingleton<StringToBoolConverter>();
            builder.Services.AddSingleton<BoolToColorConverter>();

            return builder.Build();
        }
    }
}
