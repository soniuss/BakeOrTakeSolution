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
            var baseApiUrl = "https://bakeortakesolution-production.up.railway.app"; 

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseApiUrl) });
            builder.Services.AddRefitClient<IBakeOrTakeApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseApiUrl));

             //--- Registro de ViewModels ---
            // Descomenta estas lineas para registrar tus ViewModels
             builder.Services.AddTransient<WelcomeViewModel>();
             builder.Services.AddTransient<LoginViewModel>(); 
             builder.Services.AddTransient<RegisterViewModel>();

            builder.Services.AddTransient<WelcomePage>(); // Aunque es tu página de inicio, es buena práctica
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            return builder.Build();
        }
    }
}
