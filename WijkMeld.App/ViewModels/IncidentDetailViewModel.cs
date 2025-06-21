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
        private readonly NavigationService _navigationService;

        [ObservableProperty]
        private Incident? incident;

        [ObservableProperty]
        private string incidentIdAsString;

        [ObservableProperty]
        private ObservableCollection<string> fullPhotoUrls = new();

        private Guid _currentIncidentGuidId;

        public IncidentDetailViewModel(IncidentService incidentService, AuthenticationService authenticationService, NavigationService navigationService)
        {
            _incidentService = incidentService;
            _authenticationService = authenticationService;
            Title = "Incident Details";
            _navigationService = navigationService;
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
                   
                   Debug.WriteLine("Fout", "Ongeldige incident ID.", "OK");
                    
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



                }
                else
                {
                    Debug.WriteLine($"IncidentDetailViewModel: Kon incident met ID {_currentIncidentGuidId} niet vinden.");
                  
                 
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
                            //Combine the base URI with the relative path to create the full URL
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
              
            }
            finally
            {
                IsBusy = false;
            }



        }
        [RelayCommand]
        public async Task GoBackAsync()
        {
            await _navigationService.GoToAsync("//home"); // Navigeer terug naar de vorige pagina
        }


    }
}
