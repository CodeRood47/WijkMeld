using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WijkMeld.App.Model.Enums;
using System.Threading.Tasks;
using WijkMeld.App.Model; 
using WijkMeld.App.Services;
using Microsoft.Maui.Controls; 
using System.Linq;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;


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

        [ObservableProperty]
        private bool isUserRole;

        [ObservableProperty]
        private bool isAdminRole;

        [ObservableProperty]
        private bool isFieldAgentRole; // Toekomstig

        [ObservableProperty]
        private bool isGuestRole;

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
          
        }
        public override async Task OnAppearingAsync()
        {
            await SetUserRolePropertiesAsync();
            await GetCurrentLocationAndCenterMapAsync();
            if (LoadIncidentsCommand.CanExecute(null))
            {
                await LoadIncidentsCommand.ExecuteAsync(null);
            }
        }

        private async Task SetUserRolePropertiesAsync()
        {
            await _authenticationService.InitializeAsync();
            var userRole = _authenticationService.CurrentUserRole;
            IsUserRole = (userRole == UserRole.USER);
            IsAdminRole = (userRole == UserRole.ADMIN);
            IsFieldAgentRole = (userRole == UserRole.FIELD_AGENT);
            IsGuestRole = (userRole == UserRole.GUEST);

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

                List<Incident>? incidentsToDisplay = null;



                switch(_authenticationService.CurrentUserRole)
                {
                    case UserRole.ADMIN:
                        Debug.WriteLine("HomeMapViewModel: Gebruiker is ADMIN, laden alle incidenten...");
                        incidentsToDisplay = await _incidentService.GetAllIncidentsAsync();
                        break;
                    case UserRole.FIELD_AGENT:
                        break;
                    case UserRole.USER:
                        Debug.WriteLine("HomeMapViewModel: Gebruiker is USER, laden eigen incidenten...");
                        incidentsToDisplay = await _incidentService.GetUserIncidentsAsync();
                        break;
                    case UserRole.GUEST:
                        incidentsToDisplay = await _incidentService.GetAllIncidentsAsync();
                        break;
                    default:
                        Debug.WriteLine("HomeMapViewModel: Onbekende rol, geen incidenten geladen.");
                        break;
                }

                if (incidentsToDisplay != null && incidentsToDisplay.Any())
                {
                   
                   
                        foreach (var incident in incidentsToDisplay)
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
                                IncidentPins.Add(pin);
                            Debug.WriteLine($"HomeMapViewModel: Dit is een pin {pin.Location.Longitude}");

                        }
                            else
                             {
                            Debug.WriteLine($"HomeMapViewModel: Incident {incident.Name} heeft geen geldige locatie.");
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
                    Debug.WriteLine("HomeMapViewModel: Geen incidenten om weer te geven (incidentsToDisplay is null of leeg).");

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomeMapViewModel: Fout bij het laden van incidenten: {ex.Message}");
            }
            finally
            {
                IsBusy = false; 
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
