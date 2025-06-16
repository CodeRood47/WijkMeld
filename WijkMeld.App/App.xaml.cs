
using Microsoft.Maui.Controls;
using WijkMeld.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace WijkMeld.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
           // MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var serviceProvider = Current.Handler.MauiContext?.Services;
            if (serviceProvider != null)
            {
                var authService = serviceProvider.GetService<AuthenticationService>();
                if (authService != null)
                {



                    _ = authService.InitializeAsync();
                }
            }


                    return new Window(new AppShell());
        }





    }
}