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
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string email;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string password;

        [ObservableProperty]
        private string errorMessage;


        public IAsyncRelayCommand LoginCommand { get; }


        public LoginViewModel(IAuthenticationService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLoginExecute);
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {

                var success = await _authService.LoginAsync(new LoginRequest
                {
                    Email = Email,
                    Password = Password

                });

                if (success)
                {
                    await _navigationService.GoToAsync("//home");
                }
                else
                {
                    ErrorMessage = "Inloggen mislukt. Controleer je gegevens.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Er is een onverwachte fout opgetreden tijdens het inloggen. Probeer het later opnieuw.";
                Debug.WriteLine($"LoginViewModel: LoginAsync exception: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
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
                    await _navigationService.GoToAsync("//home");
                }
                else
                {
                    ErrorMessage = "Anoniem melden mislukt. Probeer het later opnieuw.";
                    Debug.WriteLine($"LoginViewModel: Anoniem melden mislukt: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Er is een fout opgetreden bij anoniem melden";
                Debug.WriteLine($"LoginViewModel: Anoniem melden exception: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("LoginViewModel: LoginAnonymouslyAsync commando voltooid.");
            }
        }


        [RelayCommand]
        public async Task NavigateToRegisterAsync()
        {
            Debug.WriteLine("LoginViewModel: NavigateToRegisterAsync commando gestart.");
            IsBusy = true;
            try
            {
                await _navigationService.GoToAsync("//register");
                Debug.WriteLine("LoginViewModel: Navigatie naar registratiepagina voltooid.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginViewModel: Fout bij navigeren naar registratiepagina: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("LoginViewModel: NavigateToRegisterAsync commando voltooid.");
            }

        }

        public bool CanLoginExecute() => !string.IsNullOrWhiteSpace(Email) &&
                                 !string.IsNullOrWhiteSpace(Password) &&
                                 !IsBusy;
    }   
}
