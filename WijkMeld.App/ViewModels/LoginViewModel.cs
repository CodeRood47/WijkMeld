using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WijkMeld.App.Model;
using WijkMeld.App.Services;


namespace WijkMeld.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string errorMessage;


        public ICommand LoginCommand { get; }


        public LoginViewModel(AuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            ErrorMessage = "";

            var success = await _authService.LoginAsync(new LoginRequest
            {
                Email = Email,
                Password = Password

            });

            if (success)
            {
                await Shell.Current.GoToAsync("//home");
            }
            else
            {
                ErrorMessage = "Inloggen mislukt. Controleer je gegevens.";
            }
        }

    }
}
