using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(AppointmentId), "appointmentId")]
    public class PatientConsultationDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IConsultationService _consultationService;
        private readonly IAppointmentService _appointmentService;

        private string _appointmentId;
        private string _appointmentDate;
        private string _appointmentTime;
        private string _patientName;
        private string _patientIC;
        private string _patientPhone;
        private string _serviceType;
        private string _doctorRemarks;
        private string _nurseRemarks;
        private string _status;
        private bool _isLoading;

        public string AppointmentId
        {
            get => _appointmentId;
            set
            {
                _appointmentId = value;
                OnPropertyChanged();
                _ = LoadConsultationDetails();
            }
        }

        public string AppointmentDate
        {
            get => _appointmentDate;
            set => SetProperty(ref _appointmentDate, value);
        }

        public string AppointmentTime
        {
            get => _appointmentTime;
            set => SetProperty(ref _appointmentTime, value);
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

        public string PatientPhone
        {
            get => _patientPhone;
            set => SetProperty(ref _patientPhone, value);
        }

        public string ServiceType
        {
            get => _serviceType;
            set => SetProperty(ref _serviceType, value);
        }

        public string DoctorRemarks
        {
            get => _doctorRemarks;
            set => SetProperty(ref _doctorRemarks, value);
        }

        public string NurseRemarks
        {
            get => _nurseRemarks;
            set => SetProperty(ref _nurseRemarks, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand BackCommand { get; }

        public PatientConsultationDetailsViewModel(IConsultationService consultationService, IAppointmentService appointmentService)
        {
            _consultationService = consultationService;
            _appointmentService = appointmentService;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task LoadConsultationDetails()
        {
            if (string.IsNullOrEmpty(AppointmentId))
            {
                System.Diagnostics.Debug.WriteLine("No appointment ID provided");
                return;
            }

            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"Loading consultation details for appointment: {AppointmentId}");

                // Get appointment details
                var appointment = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
                if (appointment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Appointment {AppointmentId} not found");
                    await Shell.Current.DisplayAlert("Error", "Appointment not found", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Found appointment: {appointment.appointment_ID} - Status: {appointment.status}");

                // Set basic appointment info
                AppointmentDate = appointment.appointedAt?.ToString("dd MMMM yyyy") ?? "Unknown Date";
                AppointmentTime = appointment.appointedAt?.ToString("hh:mm tt") ?? "Unknown Time";
                PatientName = appointment.Patient?.patient_name ?? "Unknown Patient";
                PatientIC = appointment.patient_IC ?? "Unknown IC";
                PatientPhone = appointment.Patient?.patient_contact ?? "Unknown Phone";
                ServiceType = appointment.service_type?.ToString() ?? "General Consultation";
                Status = appointment.status ?? "Unknown";

                // Get consultation details (doctor and nurse remarks)
                var consultation = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(AppointmentId);
                if (consultation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found consultation details - Doctor: {consultation.DoctorRemark}, Nurse: {consultation.NurseRemark}");
                    DoctorRemarks = !string.IsNullOrWhiteSpace(consultation.DoctorRemark) 
                        ? consultation.DoctorRemark 
                        : "No doctor remarks available";
                    NurseRemarks = !string.IsNullOrWhiteSpace(consultation.NurseRemark) 
                        ? consultation.NurseRemark 
                        : "No nurse remarks available";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No consultation details found");
                    DoctorRemarks = "No doctor remarks available";
                    NurseRemarks = "No nurse remarks available";
                }

                System.Diagnostics.Debug.WriteLine("Consultation details loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading consultation details: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load consultation details: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
