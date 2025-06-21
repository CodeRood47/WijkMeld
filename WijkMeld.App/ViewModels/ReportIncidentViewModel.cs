using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WijkMeld.App.Services;
using WijkMeld.App.Model.Enums;
using WijkMeld.App.Model;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO; 
using Microsoft.Maui.Media; 
using Microsoft.Maui.ApplicationModel;

namespace WijkMeld.App.ViewModels
{
    public partial class ReportIncidentViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;
        private readonly GeolocationService _geolocationService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private Priority selectedPriority;

        [ObservableProperty]
        private double latitude;

        [ObservableProperty]
        private double longitude;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveIncidentCommand))] 
        private bool isLocationLoading;

        [ObservableProperty]
        private ObservableCollection<IncidentPhoto> incidentPhotos = new();

        public ObservableCollection<Priority> Priorities { get; } = new ObservableCollection<Priority>(Enum.GetValues(typeof(Priority)).Cast<Priority>());

        public ReportIncidentViewModel(IncidentService incidentService, GeolocationService geolocationService, INavigationService navigationService)
        {
            Title = "Melding doen";
            _incidentService = incidentService;
            _geolocationService = geolocationService;
            _navigationService = navigationService;
            selectedPriority = Priority.NORMAL;
        }

        public override async Task OnAppearingAsync()
        {
            await GetCurrentLocationCommand.ExecuteAsync(null);
        }

        [RelayCommand(CanExecute = nameof(CanGetLocation))]
        public async Task GetCurrentLocationAsync()
        {
            if (IsLocationLoading) return;

            try
            {
                IsLocationLoading = true;
                ErrorMessage = "";
                var location = await _geolocationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;
                    Debug.WriteLine($"Locatie opgehaald: Lat={Latitude}, Lng={Longitude}");
                }
                else
                {
                    ErrorMessage = "Kon huidige locatie niet ophalen.";
                    Debug.WriteLine("Fout bij ophalen huidige locatie.");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fout bij ophalen locatie: {ex.Message}";
                Debug.WriteLine($"Fout bij ophalen locatie: {ex.Message}");
            }
            finally
            {
                IsLocationLoading = false;
            }
        }

        private bool CanGetLocation()
        {
            return !IsLocationLoading;
        }


        partial void OnNameChanged(string value)
        {
            SaveIncidentCommand.NotifyCanExecuteChanged();
            Debug.WriteLine($"OnNameChanged: Name is nu '{value}'. Notifying SaveIncidentCommand.");
        }

        partial void OnDescriptionChanged(string value)
        {
            SaveIncidentCommand.NotifyCanExecuteChanged();
            Debug.WriteLine($"OnDescriptionChanged: Description is nu '{value}'. Notifying SaveIncidentCommand.");
        }

        [RelayCommand]
        public async Task TakePhotoAsync()
        {
            if(!MediaPicker.Default.IsCaptureSupported)
            {
                
                    Debug.WriteLine("Fout", "Camera is niet beschikbaar op dit apparaat.", "OK");
                
                return;
            }

            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("Toestemming geweigerd", "Je moet cameratoegang verlenen om foto's te maken.", "OK");
                        return;
                    }
                }


                FileResult photoResult = await MediaPicker.Default.CapturePhotoAsync();

                if (photoResult != null)
                { 
                    string newFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(photoResult.FileName)}";
                    string targetFilePath = Path.Combine(FileSystem.AppDataDirectory, newFileName);

                 
                    using (var sourceStream = await photoResult.OpenReadAsync())
                    using (var targetStream = File.OpenWrite(targetFilePath)) 
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }

                    var newIncidentPhoto = new IncidentPhoto
                    {
                        FileName = photoResult.FileName,
                        FilePath = targetFilePath,
                    };
                    IncidentPhotos.Add(newIncidentPhoto);
                    Debug.WriteLine($"Foto gemaakt en gekopieerd naar: {newIncidentPhoto.FilePath}");
                    SaveIncidentCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    Debug.WriteLine("Foto maken geannuleerd.");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                ErrorMessage = $"Camera functionaliteit niet ondersteund: {fnsEx.Message}";
                Debug.WriteLine("Fout", ErrorMessage, "OK");
            }
            catch (PermissionException pEx)
            {
                ErrorMessage = $"Toestemming geweigerd voor camera: {pEx.Message}";
                Debug.WriteLine("Fout", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Onverwachte fout bij maken foto: {ex.Message}";
                Debug.WriteLine("Fout", ErrorMessage, "OK");
                Debug.WriteLine($"Fout bij foto maken: {ex.Message}");
            }
        }
        [RelayCommand]
        public async Task PickPhotoAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Photos>();
                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("Toestemming geweigerd", "Je moet toegang verlenen tot je fotogalerij om een foto te kiezen.", "OK");
                        return;
                    }
                }
                FileResult photoResult = await MediaPicker.Default.PickPhotoAsync();
                if (photoResult != null)
                {
                 
                    string newFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(photoResult.FileName)}";
                    string targetFilePath = Path.Combine(FileSystem.AppDataDirectory, newFileName);

                    using (var sourceStream = await photoResult.OpenReadAsync())
                    using (var targetStream = File.OpenWrite(targetFilePath))
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }

                    var newIncidentPhoto = new IncidentPhoto
                    {
                        FileName = photoResult.FileName,
                        FilePath = targetFilePath,
                    };
                    IncidentPhotos.Add(newIncidentPhoto);
                    Debug.WriteLine($"Foto gekozen en gekopieerd naar: {newIncidentPhoto.FilePath}");
                    SaveIncidentCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    Debug.WriteLine("Foto kiezen geannuleerd.");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                ErrorMessage = $"Fotogalerij functionaliteit niet ondersteund: {fnsEx.Message}";
                Debug.WriteLine("Fout", ErrorMessage, "OK");
            }
            catch (PermissionException pEx)
            {
                ErrorMessage = $"Toestemming geweigerd voor fotogalerij: {pEx.Message}";
                Debug.WriteLine("Fout", ErrorMessage, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Onverwachte fout bij kiezen foto: {ex.Message}";
                Debug.WriteLine("Fout", ErrorMessage, "OK");
                Debug.WriteLine($"Fout bij foto kiezen: {ex.Message}");
            }
        }
        [RelayCommand]
        public void RemovePhoto(IncidentPhoto photoToRemove)
        {
            if (photoToRemove != null)
            {
                IncidentPhotos.Remove(photoToRemove);
                Debug.WriteLine($"Foto verwijderd uit lijst: {photoToRemove.FileName}");
                SaveIncidentCommand.NotifyCanExecuteChanged(); // Update the CanExecute status
            }
        }


        [RelayCommand(CanExecute = nameof(CanReportIncident))]
        public async Task SaveIncidentAsync()
        {
            if (IsBusy) return;


            try
            {
                IsBusy = true;
                ErrorMessage = "";
                var request = new CreateIncidentRequest
                {
                    Name = Name,
                    Description = Description,
                    Priority = (int)SelectedPriority,
                    Latitude = Latitude,
                    Longitude = Longitude
                };

                var incidentId = await _incidentService.CreateIncidentAsync(request);

                if (incidentId != Guid.Empty)
                {
                    Debug.WriteLine($"Incident succesvol aangemaakt met ID: {incidentId}");

                    // Upload the photos to the assigned incident
                    foreach (var photo in IncidentPhotos)
                    {
                        try
                        {
                            
                            using (Stream stream = await photo.OpenStreamForReadAsync())
                            {
                            
                                photo.PhotoStream = stream;
                                bool uploaded = await _incidentService.UploadIncidentPhotoAsync(photo, incidentId);
                                if (!uploaded)
                                {
                                    ErrorMessage += $" Fout bij uploaden foto: {photo.FileName}.";
                                    Debug.WriteLine($"Fout bij uploaden van foto {photo.FileName} voor incident {incidentId}.");
                                }
                            }

                            if (File.Exists(photo.FilePath))
                            {
                                File.Delete(photo.FilePath);
                                Debug.WriteLine($"Lokaal bestand {photo.FileName} verwijderd na upload.");
                            }
                        }
                        catch (Exception photoEx)
                        {
                            ErrorMessage += $" Fout bij verwerken foto '{photo.FileName}': {photoEx.Message}.";
                            Debug.WriteLine($"Fout bij verwerken/uploaden van foto '{photo.FileName}': {photoEx.Message}");
                        }
                    }
                    IncidentPhotos.Clear(); 

                 
                    await _navigationService.GoToAsync("//home");
                    Debug.WriteLine("Succes", "Incident en foto(s) succesvol gemeld!", "OK");
                    Debug.WriteLine("Incident en foto's succesvol verwerkt.");
                }
                else
                {
                    ErrorMessage = "Fout bij aanmaken incident. Controleer de gegevens.";
                    Debug.WriteLine("Incident aanmaken mislukt via service.");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Algemene fout bij opslaan incident: {ex.Message}";
                Debug.WriteLine($"Algemene fout bij opslaan incident: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanReportIncident()
        {
            bool isValid = !IsBusy &&
                           !string.IsNullOrWhiteSpace(Name) &&
                           !string.IsNullOrWhiteSpace(Description) &&
                           !IsLocationLoading && 
                           (Latitude != 0 || Longitude != 0);


            Debug.WriteLine($"CanReportIncident evaluatie: IsBusy={IsBusy}, Name='{Name}', Description='{Description}'");
            Debug.WriteLine($"Locatie status: IsLocationLoading={IsLocationLoading}, Lat={Latitude}, Lng={Longitude}");
            Debug.WriteLine($"Validatie resultaat: Name valid={!string.IsNullOrWhiteSpace(Name)}, Description valid={!string.IsNullOrWhiteSpace(Description)}");
            Debug.WriteLine($"Aantal foto's: {IncidentPhotos.Count}");
            Debug.WriteLine($"Final CanReportIncident: {isValid}");
            return isValid;


        }
    }
}


