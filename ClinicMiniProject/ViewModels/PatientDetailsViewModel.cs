using System.Windows.Input;
using ClinicMiniProject.Dtos; 

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(Patient), "SelectedPatient")]
    public class PatientDetailsViewModel : BindableObject
    {
        private string _patientName;
        public string PatientName
        {
            get => _patientName;
            set { _patientName = value; OnPropertyChanged(); }
        }

        private string _queueId;
        public string QueueId
        {
            get => _queueId;
            set { _queueId = value; OnPropertyChanged(); }
        }

        private string _icNumber;
        public string IcNumber
        {
            get => _icNumber;
            set { _icNumber = value; OnPropertyChanged(); }
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
                    QueueId = _patient.QueueId;
                    IcNumber = _patient.ICNumber;
                }
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
            await Application.Current.MainPage.DisplayAlert("Action", "Opening Edit Form...", "OK");
        }
    }
}