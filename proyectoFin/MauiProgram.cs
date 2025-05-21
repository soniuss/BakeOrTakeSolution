using Microsoft.Extensions.Logging;

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
            var baseApiUrl = "https://tu-api-railway.railway.app"; // ¡Reemplaza con tu URL real!

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(baseApiUrl) });
            builder.Services.AddRefitClient<IBakeOrTakeApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseApiUrl));

            // --- Registro de ViewModels ---
            // Descomenta estas lineas para registrar tus ViewModels
            builder.Services.AddTransient<WelcomeViewModel>();
            // builder.Services.AddTransient<LoginViewModel>(); 
            // builder.Services.AddTransient<RegisterViewModel>(); 

            return builder.Build();
        }
    }
}
