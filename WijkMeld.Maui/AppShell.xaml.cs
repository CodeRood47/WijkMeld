
using WijkMeld.Maui.Views;


namespace WijkMeld.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("login", typeof(LoginView));
        }
    }
}
