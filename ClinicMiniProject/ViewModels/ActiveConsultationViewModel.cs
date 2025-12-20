using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(AppointmentId), "appointmentId")]
    public class ActiveConsultationViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;
        private readonly IAppointmentService _appointmentService;

        private string _appointmentId;
        private string _patientName;
        private string _patientIC;
        private string _serviceType;
        private string _consultationRemark = string.Empty;
        private string _status = "In Progress";

        public string AppointmentId
        {
            get => _appointmentId;
            set
            {
                _appointmentId = value;
                OnPropertyChanged();
                _ = LoadAppointmentDetails();
            }
        }

        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        public string PatientIC
        {
            get => _patientIC;
            set => SetProperty(ref _patientIC, value);
        }

        public string ServiceType
        {
            get => _serviceType;
            set => SetProperty(ref _serviceType, value);
        }

        public string ConsultationRemark
        {
            get => _consultationRemark;
            set => SetProperty(ref _consultationRemark, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ICommand BackCommand { get; }
        public ICommand EndConsultationCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ActiveConsultationViewModel(IAuthService authService, IConsultationService consultationService, IAppointmentService appointmentService)
        {
            _authService = authService;
            _consultationService = consultationService;
            _appointmentService = appointmentService;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            EndConsultationCommand = new Command(async () => await EndConsultation(), () => !string.IsNullOrWhiteSpace(ConsultationRemark));
            
            // Update command state when remark changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ConsultationRemark))
                {
                    (EndConsultationCommand as Command)?.ChangeCanExecute();
                }
            };
        }

        private async Task LoadAppointmentDetails()
        {
            if (string.IsNullOrEmpty(AppointmentId)) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Loading appointment details for ID: {AppointmentId}");
                
                var appointment = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
                if (appointment != null)
                {
                    PatientName = appointment.Patient?.patient_name ?? "Unknown";
                    PatientIC = appointment.patient_IC;
                    ServiceType = appointment.service_type ?? "General Consultation";
                    Status = "Consultation In Progress";
                    
                    System.Diagnostics.Debug.WriteLine($"Loaded: {PatientName}, IC: {PatientIC}, Service: {ServiceType}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Appointment not found");
                    await Shell.Current.DisplayAlert("Error", "Appointment not found", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading appointment: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load appointment details: {ex.Message}", "OK");
            }
        }

        private async Task EndConsultation()
        {
            if (string.IsNullOrWhiteSpace(ConsultationRemark))
            {
                await Shell.Current.DisplayAlert("Required", "Please enter consultation remarks before ending", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "End Consultation", 
                "Are you sure you want to end this consultation?", 
                "Yes", 
                "No");

            if (!confirm) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Ending consultation for appointment: {AppointmentId}");
                System.Diagnostics.Debug.WriteLine($"Doctor remarks: {ConsultationRemark}");
                
                // Save doctor remarks to database
                await _consultationService.EndConsultationAsync(AppointmentId, ConsultationRemark, null);
                
                Status = "Consultation Completed";
                
                await Shell.Current.DisplayAlert("Success", "Consultation ended successfully. Nurse can now add their remarks.", "OK");
                
                // Navigate back to dashboard
                await Shell.Current.GoToAsync("///DoctorDashboardPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ending consultation: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to end consultation: {ex.Message}", "OK");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
