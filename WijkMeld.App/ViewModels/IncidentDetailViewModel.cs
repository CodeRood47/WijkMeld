using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using WijkMeld.App.Services;
using WijkMeld.App.Model;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace WijkMeld.App.ViewModels
{
    [QueryProperty(nameof(IncidentIdAsString), "incidentId")]
    public partial class IncidentDetailViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;
        private readonly AuthenticationService _authenticationService;

        [ObservableProperty]
        private Incident? incident;

        [ObservableProperty]
        private string incidentIdAsString;

        [ObservableProperty]
        private ObservableCollection<string> fullPhotoUrls = new();

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

            // try to parse the string value to a Guid
            if (Guid.TryParse(value, out Guid parsedGuid))
            {
                _currentIncidentGuidId = parsedGuid; // save parcing guid
                Debug.WriteLine($"IncidentDetailViewModel: Parsed Guid: {_currentIncidentGuidId}");

                if (_currentIncidentGuidId != Guid.Empty)
                {
                    await LoadIncidentDetailsAsync(); // call the method to load incident details that uses the parsed Guid
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige incident ID.", "OK");
                    });
                }
            }
            else
            { 
                _currentIncidentGuidId = Guid.Empty; 
                Debug.WriteLine($"IncidentDetailViewModel: Failed to parse '{value}' into a Guid.");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige incident ID.", "OK");
                });
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


                    //if (Incident.UserId != null)
                    //{
                    //    Debug.WriteLine($"IncidentDetailViewModel: User object is NOT null.");
                    //    Debug.WriteLine($"IncidentDetailViewModel: User Name: {Incident.user.UserName ?? "NULL or Empty"}");
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"IncidentDetailViewModel: User object IS null. Cannot display user name.");
                    //}
                }
                else
                {
                    Debug.WriteLine($"IncidentDetailViewModel: Kon incident met ID {_currentIncidentGuidId} niet vinden.");
                  
                    await Shell.Current.DisplayAlert("Fout", "Het opgevraagde incident kon niet worden gevonden.", "OK");
                }
                FullPhotoUrls.Clear();
                
                if (Incident.PhotoFilePaths != null && Incident.PhotoFilePaths.Any())
                {
                    Uri apiBaseUri = _incidentService.GetApiBaseAddress();
                    Debug.WriteLine($"IncidentDetailViewModel: Aantal foto's opgehaald: {Incident.PhotoFilePaths.Count}");
                    if (apiBaseUri != null)
                    {
                        foreach (var relativePath in Incident.PhotoFilePaths)
                        {
                            // Combineer de basis-URL met het relatieve pad
                            // Zorg ervoor dat de relativePath niet al een absolute URL is als het al een domain bevat
                            // en dat apiBaseUri eindigt met een '/' en relativePath begint met een '/'
                            string fullUrl = new Uri(apiBaseUri, relativePath).ToString();
                            FullPhotoUrls.Add(fullUrl);
                            Debug.WriteLine($"Added full photo URL: {fullUrl}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("IncidentDetailViewModel: Kon API BaseAddress niet ophalen om foto URL's te construeren.");
                       
                    }
                }
                else
                {
                    Debug.WriteLine($"IncidentDetailViewModel: Geen foto's gevonden voor incident.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncidentDetailViewModel: Fout bij laden incident details: {ex.Message}");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Er is een fout opgetreden bij het laden van de incidentdetails.", "OK");
                });
              
            }
            finally
            {
                IsBusy = false;
            }



        }
        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("//home"); // Navigeer terug naar de vorige pagina
        }


    }
}
