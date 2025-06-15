using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WijkMeld.App.Model;
using WijkMeld.App.Services;
using System.Diagnostics;


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

        [RelayCommand]
        public async Task LoginAnonymouslyAsync()
        {
            Debug.WriteLine("LoginViewModel: LoginAnonymouslyAsync commando gestart.");
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var success = await _authService.LoginAsGuestAsync();

                if (success)
                {
                    Debug.WriteLine("LoginViewModel: Anonieme login succesvol, navigeert naar home.");
                    await Shell.Current.GoToAsync("//home");
                }
                else
                {
                    ErrorMessage = "Anoniem melden mislukt. Probeer het later opnieuw.";
                    Debug.WriteLine($"LoginViewModel: Anoniem melden mislukt: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Er is een fout opgetreden bij anoniem melden: {ex.Message}";
                Debug.WriteLine($"LoginViewModel: Anoniem melden exception: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("LoginViewModel: LoginAnonymouslyAsync commando voltooid.");
            }
        }

    }
}
