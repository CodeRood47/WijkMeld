using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WijkMeld.App.Model; 
using WijkMeld.App.Services;
using Microsoft.Maui.Controls; 
using System.Linq;


namespace WijkMeld.App.ViewModels
{
    public partial class HomeMapViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;
        private readonly AuthenticationService _authenticationService;

        [ObservableProperty]
        private ObservableCollection<Incident> incidents;

        public HomeMapViewModel(IncidentService incidentService, AuthenticationService authenticationService)
        {
            _incidentService = incidentService;
            _authenticationService = authenticationService;
            Incidents = new ObservableCollection<Incident>();
            Title = "Mijn Incidenten";
        }
        public override async Task OnAppearingAsync()
        {
            if (LoadIncidentsCommand.CanExecute(null))
            {
                await LoadIncidentsCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand(CanExecute = nameof(CanLoadIncidents))]
        public async Task LoadIncidentsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Incidents.Clear();

                if (_authenticationService.IsUserLoggedIn())
                {
                    System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Gebruiker is ingelogd, laden incidenten...");
                    var userIncidents = await _incidentService.GetUserIncidentsAsync();
                    if (userIncidents != null && userIncidents.Any())
                    {
                        foreach (var incident in userIncidents)
                        {
                            Incidents.Add(incident);
                        }
                        System.Diagnostics.Debug.WriteLine($"HomeMapViewModel: {Incidents.Count} incidenten geladen.");

                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Geen incidenten gevonden voor de gebruiker.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Gebruiker is niet ingelogd, kan geen incidenten laden.");
                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomeMapViewModel: Fout bij het laden van incidenten: {ex.Message}");
            }
            finally
            {
                IsBusy = false; // Stelt de IsBusy property van BaseViewModel in
            }

        }


        private bool CanLoadIncidents()
        {
            return !IsBusy;
        }

        [RelayCommand]
        public async Task NavigateToIncidentDetailsAsync(Incident incident)
        {
            if (incident == null) return;
            await Shell.Current.GoToAsync($"incidentdetails?incidentId={incident.Id}");
        }

        [RelayCommand]
        public async Task LogoutCommandAsync()
        {
            await _authenticationService.LogoutAsync();
    
            await Shell.Current.GoToAsync("//login");
            System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Uitgelogd en navigeert naar LoginView.");
        }

        [RelayCommand]
        public async Task NavigateToReportIncidentAsync()
        { 
            await Shell.Current.GoToAsync("//reportincident"); 
            System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Navigeert naar ReportIncidentView.");
        }
    }
}
