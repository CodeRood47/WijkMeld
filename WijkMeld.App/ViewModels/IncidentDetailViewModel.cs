using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using WijkMeld.App.Services;
using WijkMeld.App.Model;
using WijkMeld.App.Model.Enums;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace WijkMeld.App.ViewModels
{
    [QueryProperty(nameof(IncidentIdAsString), "incidentId")]
    public partial class IncidentDetailViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;
        private readonly AuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Incident? incident;

        [ObservableProperty]
        private string incidentIdAsString;

        [ObservableProperty]
        private ObservableCollection<string> fullPhotoUrls = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateIncidentCommand))]
        private Status selectedStatus;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateIncidentCommand))]
        private Priority selectedPriority;

        [ObservableProperty]
        private bool isAdmin;

        [ObservableProperty]
        private string updateNote = string.Empty;

        private Guid _currentIncidentGuidId;

        public IncidentDetailViewModel(IncidentService incidentService, AuthenticationService authenticationService, INavigationService navigationService)
        {
            _incidentService = incidentService;
            _authenticationService = authenticationService;
            Title = "Incident Details";
            _navigationService = navigationService;
            SelectedStatus = Status.GEMELD;
            SelectedPriority = Priority.LOW;
        }

        public override async Task OnAppearingAsync()
        {
            await base.OnAppearingAsync(); 
        
            await CheckUserAdminStatusAsync();

            if (Incident == null && _currentIncidentGuidId != Guid.Empty)
            {
                await LoadIncidentDetailsAsync();
            }
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
        [RelayCommand(CanExecute = nameof(CanUpdateIncident))]
        public async Task UpdateIncidentAsync()
        {
            string ErrorMessage = string.Empty;
            if (IsBusy || Incident == null)
            {
                return; 
            }
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                Debug.WriteLine($"IncidentDetailViewModel: Start UpdateIncidentAsync voor ID: {_currentIncidentGuidId}");

                Status? statusToSend = null;
                if (Incident.Status != SelectedStatus)
                {
                    statusToSend = SelectedStatus;
                    Debug.WriteLine($"Status gewijzigd van {Incident.Status} naar {SelectedStatus}");
                }
                else
                {
                    Debug.WriteLine($"Status ongewijzigd: {SelectedStatus}");
                }

                Priority? priorityToSend = null;
                if (Incident.Priority != SelectedPriority)
                {
                    priorityToSend = SelectedPriority;
                    Debug.WriteLine($"Prioriteit gewijzigd van {Incident.Priority} naar {SelectedPriority}");
                }
                else
                {
                    Debug.WriteLine($"Prioriteit ongewijzigd: {SelectedPriority}");
                }

                string? noteToSend = string.IsNullOrWhiteSpace(UpdateNote) ? null : UpdateNote;

                if (!statusToSend.HasValue && !priorityToSend.HasValue && string.IsNullOrWhiteSpace(noteToSend))
                {
                    ErrorMessage = "Geen wijzigingen gedetecteerd of notitie ingevoerd om op te slaan.";
                    Debug.WriteLine("UpdateIncidentAsync: Geen relevante wijzigingen gedetecteerd.");
                    return;
                }

             
                bool success = await _incidentService.UpdateIncidentStatusAndPriorityAsync(
                    _currentIncidentGuidId,
                    statusToSend,
                    priorityToSend,
                    noteToSend
                );

                if (success)
                {
                    Debug.WriteLine("UpdateIncidentAsync: Status/prioriteit succesvol bijgewerkt.");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Succes", "Incident status en/of prioriteit succesvol bijgewerkt!", "OK");
                    });
                    UpdateNote = string.Empty; 
                    await LoadIncidentDetailsAsync(); 
                }
                else
                {
                    ErrorMessage = "Fout bij het bijwerken van de incident status/prioriteit. Controleer logs.";
                    Debug.WriteLine("UpdateIncidentAsync: Fout bij bijwerken status/prioriteit via service.");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Fout", ErrorMessage, "OK");
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Algemene fout bij bijwerken incident: {ex.Message}";
                Debug.WriteLine($"UpdateIncidentAsync: Algemene fout: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", ErrorMessage, "OK");
                });
            }
            finally
            {
                IsBusy = false;
                UpdateIncidentCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanUpdateIncident()
        {
        
            bool hasIncidentLoaded = Incident != null && Incident.Id != Guid.Empty;

            bool statusOrPriorityChanged = false;
            if (Incident != null)
            {
                statusOrPriorityChanged = Incident.Status != SelectedStatus || Incident.Priority != SelectedPriority;
            }

            bool hasNote = !string.IsNullOrWhiteSpace(UpdateNote);

            return hasIncidentLoaded && !IsBusy && IsAdmin && (statusOrPriorityChanged || hasNote);
        }


        private async Task CheckUserAdminStatusAsync()
        {
            var Role = await _authenticationService.GetUserRoleAsync();
            IsAdmin = Role == "ADMIN";

            Debug.WriteLine($"IncidentDetailViewModel: IsAdmin = {IsAdmin}");

            Debug.WriteLine($"IncidentDetailViewModel: Verwachte Admin Role vergelijking = 'ADMIN'");
            Debug.WriteLine($"IncidentDetailViewModel: IsAdmin na vergelijking = {IsAdmin}");

            UpdateIncidentCommand.NotifyCanExecuteChanged();
        }
        [RelayCommand]
        public async Task GoBackAsync()
        {
            await _navigationService.GoToAsync("//home"); // Navigate back to home
        }


    }
}
