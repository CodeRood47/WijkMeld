using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WijkMeld.App.Model; 
using WijkMeld.App.Services;
using Microsoft.Maui.Controls; 
using System.Linq;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;


namespace WijkMeld.App.ViewModels
{
    public partial class HomeMapViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;
        private readonly AuthenticationService _authenticationService;
        private readonly GeolocationService _geolocationService;

        [ObservableProperty]
        private ObservableCollection<Incident> incidents;

        [ObservableProperty]
        private ObservableCollection<Pin> incidentPins;

        [ObservableProperty]
        private MapSpan currentMapRegion;

        public HomeMapViewModel(IncidentService incidentService, AuthenticationService authenticationService, GeolocationService geolocationService)
        {
            _incidentService = incidentService;
            _authenticationService = authenticationService;
            _geolocationService = geolocationService;
            Incidents = new ObservableCollection<Incident>();
            IncidentPins = new ObservableCollection<Pin>();
            Title = "Mijn Incidenten";


            CurrentMapRegion = MapSpan.FromCenterAndRadius(
                new Microsoft.Maui.Devices.Sensors.Location(52.370216, 4.895168), 
                Distance.FromKilometers(10) 
            );
            _geolocationService = geolocationService;
        }
        public override async Task OnAppearingAsync()
        {
            await GetCurrentLocationAndCenterMapAsync();
            if (LoadIncidentsCommand.CanExecute(null))
            {
                await LoadIncidentsCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        public async Task GetCurrentLocationAndCenterMapAsync()
        {
            try
            {
                var currentLocation = await _geolocationService.GetCurrentLocationAsync();
                if (currentLocation != null)
                {
                        CurrentMapRegion = MapSpan.FromCenterAndRadius
                        (
                        currentLocation,
                        Distance.FromKilometers(10) 
                        );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Geen huidige locatie beschikbaar.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomeMapViewModel: Fout bij ophalen huidige locatie voor kaart: {ex.Message}");
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
                IncidentPins.Clear();

                if (_authenticationService.IsUserLoggedIn())
                {
                    System.Diagnostics.Debug.WriteLine("HomeMapViewModel: Gebruiker is ingelogd, laden incidenten...");
                    var userIncidents = await _incidentService.GetUserIncidentsAsync();
                    if (userIncidents != null && userIncidents.Any())
                    {
                        foreach (var incident in userIncidents)
                        {
                            Incidents.Add(incident);

                            if(incident.Location != null)
                            {
                                var pin = new Pin()
                                {
                                    Label = incident.Name,
                                    Address = incident.Location.Address,
                                    Location = new Microsoft.Maui.Devices.Sensors.Location(incident.Location.lat, incident.Location.lng),
                                    Type = PinType.Generic
                                };
                                IncidentPins.Append(pin);
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"HomeMapViewModel: {Incidents.Count} incidenten geladen.");

                        if(IncidentPins.Any())
                        {
                            currentMapRegion = MapSpan.FromCenterAndRadius(
                                incidentPins.First().Location, Distance.FromKilometers(10)); 
                        }
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
        public async Task LogoutAsync()
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
