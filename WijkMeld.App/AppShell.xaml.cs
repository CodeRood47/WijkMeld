using Microsoft.Maui.Controls;
using WijkMeld.App.Views;
namespace WijkMeld.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("login", typeof(LoginView));
            Routing.RegisterRoute("home", typeof(HomeMapView));
            Routing.RegisterRoute("reportincident", typeof(ReportIncidentView));
            Routing.RegisterRoute("incidentdetails", typeof(IncidentDetailView));
            Routing.RegisterRoute("register", typeof(RegisterView));
        }
    }
}
