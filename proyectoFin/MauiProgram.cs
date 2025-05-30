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

            //--- Registro de ViewModels ---
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>(); // Transient porque cada registro es nuevo
            builder.Services.AddTransient<ClientMainViewModel>(); // Transient para que cada vez sea una nueva instancia
            builder.Services.AddTransient<EmpresaMainViewModel>(); // Transient
            builder.Services.AddTransient<RecetaDetalleViewModel>(); // Transient
            builder.Services.AddTransient<WelcomeViewModel>(); // Transient si se recrea al cerrar sesión

            // Registro de Páginas (la mayoría AddTransient)
            builder.Services.AddTransient<LoginPage>(); // Transient si se vuelve a crear
            builder.Services.AddTransient<RegisterPage>(); // Transient
            builder.Services.AddTransient<WelcomePage>(); // Transient
            builder.Services.AddTransient<ClientMainPage>(); // Transient
            builder.Services.AddTransient<EmpresaMainPage>(); // Transient
            builder.Services.AddTransient<RecetaDetallePage>(); // Transient


            return builder.Build();
        }
    }
}
