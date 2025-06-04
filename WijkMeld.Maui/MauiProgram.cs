using Microsoft.Extensions.Logging;
using WijkMeld.Maui.Services;
using WijkMeld.Maui.ViewModels;
using WijkMeld.Maui.Views;
using WijkMeld.Maui.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;




namespace WijkMeld.Maui
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
           

            //builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddHttpClient<AuthenticationService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddSingleton<LoginView>();
            builder.Services.AddSingleton<App>();




#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
