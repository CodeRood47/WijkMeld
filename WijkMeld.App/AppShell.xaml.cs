using Microsoft.Maui.Controls;
using WijkMeld.App.Views;
namespace WijkMeld.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Login", typeof(LoginView));
            Routing.RegisterRoute("home", typeof(HomeMapView));
        }
    }
}
