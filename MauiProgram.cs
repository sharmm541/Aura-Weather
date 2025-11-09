using Microsoft.Extensions.Logging;
using Aura.Services;
using Aura.ViewModels;

namespace Aura
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


            // Регистрируем сервисы
            builder.Services.AddSingleton<IWeatherService, WeatherService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddTransient<MainViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

    }
}