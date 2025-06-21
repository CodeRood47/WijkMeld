using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Services;
using WijkMeld.App.Model;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace WijkMeld.App.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authenticationService;



        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string email;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string password;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string confirmPassword;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string userName;

        [ObservableProperty]
        private string errorMessage = string.Empty;
        public RegisterViewModel(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            Title = "Registreren";
        }
        [RelayCommand(CanExecute = nameof(CanRegister))]
        public async Task RegisterAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;



            try
            {
                Debug.WriteLine("Start registratieproces");

                if (!IsValidInput())
                {
                    Debug.WriteLine($"Validatie Mislukt: {ErrorMessage}");
                    return;
                }

                Debug.WriteLine($"Registratie aanvraag voor: {UserName}");

                var registerRequest = new RegisterRequest
                {
                    UserName = UserName,
                    Email = Email,
                    Password = Password, // Is call passwordHash beceause, it is called that in the API
                    Role = 0 // Default role, can be changed later
                };

                Debug.WriteLine($"RegisterRequest aangemaakt: UserName='{registerRequest.UserName}', Email='{registerRequest.Email}', Role={registerRequest.Role}");


                var result = await _authenticationService.RegisterUserAsync(registerRequest);

                if (result)
                {
                    await Application.Current.MainPage.DisplayAlert("Succes", "Registratie succesvol.", "OK");
                    // Navigate to login view
                    await Shell.Current.GoToAsync("//login");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Registratie mislukt. Probeer het opnieuw.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                RegisterCommand.NotifyCanExecuteChanged();
            }
        }
        private bool CanRegister()
        {
            bool hasValidInput = !string.IsNullOrWhiteSpace(Email) &&
                                  !string.IsNullOrWhiteSpace(Password) &&
                                  !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                                  !string.IsNullOrWhiteSpace(UserName) &&
                                  Password == ConfirmPassword &&
                                  !IsBusy;
            Debug.WriteLine($"CanRegister evaluatie: Email='{Email}', Pwd='{Password}', Confirm='{ConfirmPassword}', User='{UserName}', IsBusy={IsBusy}. Result: {hasValidInput}");
            return hasValidInput;
        }

        private bool IsValidInput()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword) || string.IsNullOrWhiteSpace(UserName))
            {
                ErrorMessage = "Vul alle verplichte velden in.";
                return false;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Wachtwoorden komen niet overeen.";
                return false;
            }


            return true;
        }
    }   

}     




    

