using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using WijkMeld.App.Services;
using WijkMeld.App.ViewModels;
using WijkMeld.App.Views;

namespace WijkMeld.App
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

           

            builder.Services.AddSingleton(sp =>
            {
                var baseAdress = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7226"
                : "https://localhost:7226";

                return new HttpClient { BaseAddress = new Uri(baseAdress) };
            });

            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
