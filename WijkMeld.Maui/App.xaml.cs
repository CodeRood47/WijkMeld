namespace WijkMeld.Maui
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();

            // Stel LoginView als startpagina in via DI
            shell.GoToAsync("//login");

            return new Window(shell);
        }
    }
}