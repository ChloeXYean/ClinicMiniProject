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

        private string registeredTime = string.Empty;
        public string RegisteredTime
        {
            get => registeredTime;
            set { registeredTime = value; OnPropertyChanged(); }
        }

        private string phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => phoneNumber;
            set { phoneNumber = value; OnPropertyChanged(); }
        }

        private PatientQueueDto _patient;
        public PatientQueueDto Patient
        {
            get => _patient;
            set
            {
                _patient = value;
                OnPropertyChanged();
                if (_patient != null)
                {
                    PatientName = _patient.PatientName;
                    QueueNo = _patient.QueueId;
                    IcNumber = _patient.ICNumber;

                    // 2. MAP VALUES HERE so the UI updates
                    RegisteredTime = _patient.RegisteredTime;
                    PhoneNumber = _patient.PhoneNumber;
                }
            }
        }

        // --- Commands ---
        public ICommand BackCommand { get; }
        public ICommand UpdateCommand { get; }

        public PatientDetailsViewModel()
        {

            PatientName = "Loading...";
            QueueNo = "--";
            IcNumber = "--";

            BackCommand = new Command(async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("..");
                });
            });

            UpdateCommand = new Command(OnUpdate);
        }

        private async void OnUpdate()
        {
            await Shell.Current.DisplayAlert("Action", "Opening Edit Form...", "OK");
        }
    }
}