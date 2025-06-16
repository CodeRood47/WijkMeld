using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using WijkMeld.App.Services;
using WijkMeld.App.Model;


namespace WijkMeld.App.ViewModels
{
    [QueryProperty(nameof(IncidentIdAsString), "incidentId")]
    public partial class IncidentDetailViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;
        private readonly AuthenticationService _authenticationService;

        [ObservableProperty]
        private Incident incident;

        [ObservableProperty]
        private string incidentIdAsString;

        private Guid _currentIncidentGuidId;

        public IncidentDetailViewModel(IncidentService incidentService, AuthenticationService authenticationService)
        {
            _incidentService = incidentService;
            _authenticationService = authenticationService;
            Title = "Incident Details";
        }

        async partial void OnIncidentIdAsStringChanged(string value)
        {
            Debug.WriteLine($"IncidentDetailViewModel: OnIncidentIdAsStringChanged triggered with string value: {value}");

            // Probeer de string waarde naar een Guid te parsen
            if (Guid.TryParse(value, out Guid parsedGuid))
            {
                _currentIncidentGuidId = parsedGuid; // Sla de geparseerde Guid op
                Debug.WriteLine($"IncidentDetailViewModel: Parsed Guid: {_currentIncidentGuidId}");

                if (_currentIncidentGuidId != Guid.Empty)
                {
                    await LoadIncidentDetailsAsync(); // Roep de laadmethode aan met de geparseerde Guid
                }
                else
                {
                    Debug.WriteLine("IncidentDetailViewModel: Parsed IncidentId is Guid.Empty, not loading details.");
                }
            }
            else // Dit is het correcte else-blok voor de TryParse
            {
                _currentIncidentGuidId = Guid.Empty; // Zet naar Guid.Empty als parsen mislukt
                Debug.WriteLine($"IncidentDetailViewModel: Failed to parse '{value}' into a Guid.");
                await Shell.Current.DisplayAlert("Fout", $"Ongeldige incident ID: '{value}'", "OK");
            }
        }

        public async Task LoadIncidentDetailsAsync()
        {
            if(IsBusy) return;

            try
            {
                IsBusy = true;

                var result = await _incidentService.GetIncidentByIdAsync(_currentIncidentGuidId);
                if (result != null)
                {
                    Incident = result;
                    Debug.WriteLine($"IncidentDetailViewModel: Incident loaded successfully. Name: {Incident.Name}");


                    if (Incident.user != null)
                    {
                        Debug.WriteLine($"IncidentDetailViewModel: User object is NOT null.");
                        Debug.WriteLine($"IncidentDetailViewModel: User Name: {Incident.user.UserName ?? "NULL or Empty"}");
                    }
                    else
                    {
                        Debug.WriteLine($"IncidentDetailViewModel: User object IS null. Cannot display user name.");
                    }
                }
                else
                {
                    Debug.WriteLine($"IncidentDetailViewModel: Kon incident met ID {_currentIncidentGuidId} niet vinden.");
                    // Hier kun je een foutmelding tonen aan de gebruiker
                    await Shell.Current.DisplayAlert("Fout", "Het opgevraagde incident kon niet worden gevonden.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncidentDetailViewModel: Fout bij laden incident details: {ex.Message}");
                // Hier kun je een foutmelding tonen aan de gebruiker
                await Shell.Current.DisplayAlert("Fout", "Er is een fout opgetreden bij het laden van de incidentdetails.", "OK");
            }
            finally
            {
                IsBusy = false;
            }



        }


    }
}
