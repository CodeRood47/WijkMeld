using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Maps;
using CommunityToolkit.Mvvm.ComponentModel;
using WijkMeld.App.Services;
using WijkMeld.App.ViewModels;
using WijkMeld.App.Views;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Net.Security; // Nodig voor SslPolicyErrors
using Microsoft.Maui.Devices; // Nodig voor DeviceInfo.Platform
using Microsoft.Extensions.Configuration; // Nieuw: Nodig voor IConfiguration
using System.Reflection; 
using WijkMeld.App.Configuration; // Nieuw: Nodig voor HttpClientConfigurator (aannemende dat je het daar plaatst)
using System.Diagnostics;
namespace WijkMeld.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
#if WINDOWS
                .UseMauiCommunityToolkitMaps("key") // Zorg dat je 'key' een geldige API-key is
#else
                .UseMauiMaps()
#endif
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string apiUrl;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
              
                apiUrl = "http://10.0.2.2:5079";
            }
            else 
            {
                
                apiUrl = "https://localhost:7226";
            }

     
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException($"De API URL kon niet worden vastgesteld voor het huidige platform.");
            }


            HttpClientConfigurator.Configure(builder.Services, apiUrl);
            //builder.Services.AddHttpClient("ApiClient", httpClient =>
            //{
            //    httpClient.BaseAddress = new Uri(apiUrl);
            //});


            builder.Services.AddTransient<IncidentService>();
            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddSingleton<GeolocationService>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeMapViewModel>();
            builder.Services.AddTransient<ReportIncidentViewModel>();
            builder.Services.AddTransient<IncidentDetailViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();

            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<HomeMapView>();
            builder.Services.AddTransient<ReportIncidentView>();
            builder.Services.AddTransient<IncidentDetailView>();
            builder.Services.AddTransient<RegisterView>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}