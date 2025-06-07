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

namespace WijkMeld.App.ViewModels
{
    public partial class ReportIncidentViewModel : BaseViewModel
    {
        private readonly IncidentService _incidentService;

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

        public ObservableCollection<Priority> Priorities { get; } = new ObservableCollection<Priority>(Enum.GetValues(typeof(Priority)).Cast<Priority>());

        public ReportIncidentViewModel(IncidentService incidentService)
        {
            Title = "Melding doen";
            _incidentService = incidentService;
            selectedPriority = Priority.NORMAL;
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
                    Latitude = 0,
                    Longitude = 0
                };

                var success = await _incidentService.CreateIncidentAsync(request);

                if(success)
                {
                    await Shell.Current.GoToAsync("//home");
                    Debug.WriteLine("Incident succesvol aangemaakt.");
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

            Debug.WriteLine($"CanReportIncident evaluatie: IsBusy={IsBusy}, Name='{Name}', Description='{Description}'");
            Debug.WriteLine($"Validatie resultaat: Name valid={!string.IsNullOrWhiteSpace(Name)}, Description valid={!string.IsNullOrWhiteSpace(Description)}");
            return !IsBusy && !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Description);
                
        }
    }
}


