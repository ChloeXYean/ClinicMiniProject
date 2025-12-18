using System.Windows.Input;
using ClinicMiniProject.Dtos; 

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(Patient), "SelectedPatient")]
    public class PatientDetailsViewModel : BindableObject
    {
        private string patientName = string.Empty;
        public string PatientName { 
            get => patientName; 
            set { 
                patientName = value; 
                OnPropertyChanged(); 
            } 
        }

        private string queueNo = string.Empty;
        public string QueueNo { 
            get => queueNo; 
            set { queueNo = value; 
                OnPropertyChanged(); 
            } 
        }

        private string icNumber = string.Empty;
        public string IcNumber { 
            get => icNumber; 
            set { icNumber = value; 
                OnPropertyChanged(); 
            } 
        }

        // --- Commands ---
        public ICommand BackCommand { get; }
        public ICommand UpdateCommand { get; }

        public PatientDetailsViewModel()
        {

            PatientName = "Loading...";
            QueueId = "--";
            IcNumber = "--";

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            UpdateCommand = new Command(OnUpdate);
        }

        private async void OnUpdate()
        {
            await Shell.Current.DisplayAlert("Action", "Opening Edit Form...", "OK");
        }
    }
}