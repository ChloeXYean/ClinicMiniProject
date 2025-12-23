using System.Collections.ObjectModel;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(AppointmentId), "appointmentId")]
    public class ConsultationDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;
        private readonly IAppointmentService _appointmentService;

        private string _appointmentId;
        private string _appointmentDate;
        private string _appointmentTime;
        private string _patientName;
        private string _patientIC;
        private string _serviceType;
        private string _doctorRemarks;
        private string _nurseNotes;
        private string _prescription;
        private string _status;

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

        public string NurseNotes
        {
            get => _nurseNotes;
            set => SetProperty(ref _nurseNotes, value);
        }

        public string Prescription
        {
            get => _prescription;
            set => SetProperty(ref _prescription, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ICommand BackCommand { get; }

        public ConsultationDetailsViewModel(IAuthService authService, IConsultationService consultationService, IAppointmentService appointmentService)
        {
            _authService = authService;
            _consultationService = consultationService;
            _appointmentService = appointmentService;

            BackCommand = new Command(async () => 
            {
                if (_authService.GetCurrentPatient() != null)
                {
                    await Shell.Current.GoToAsync("PatientAppointmentHistory?UserType=Patient");
                }
                else
                {
                    await Shell.Current.GoToAsync("///AppointmentHistoryPage?UserType=Doctor");
                }
            });
        }

        private async Task LoadConsultationDetails()
        {
            if (string.IsNullOrEmpty(AppointmentId)) return;

            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
                if (appointment != null)
                {
                    AppointmentDate = appointment.appointedAt?.ToString("dd MMM yyyy") ?? " - ";
                    AppointmentTime = appointment.appointedAt?.ToString("hh:mm tt") ?? " - ";
                    PatientName = appointment.Patient?.patient_name ?? " - ";
                    PatientIC = appointment.patient_IC ?? " - ";
                    ServiceType = appointment.service_type?.ToString() ?? "General Consultation";
                    Status = appointment.status ?? " - ";

                    // Load consultation details if available
                    var consultation = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(AppointmentId);
                    if (consultation != null)
                    {
                        DoctorRemarks = !string.IsNullOrWhiteSpace(consultation.DoctorRemark) ? consultation.DoctorRemark : "No doctor remarks available ";
                        NurseNotes = !string.IsNullOrWhiteSpace(consultation.NurseRemark) ? consultation.NurseRemark : "No Nurse remarks available";
                        Prescription = " - "; // Or fetch prescription if you have that logic later
                    }
                    else
                    {
                        DoctorRemarks = " - ";
                        NurseNotes = " - ";
                        Prescription = " - ";
                    }
                
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading consultation details: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load consultation details.", "OK");
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