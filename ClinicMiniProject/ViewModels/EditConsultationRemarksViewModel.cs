using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(AppointmentId), "appointmentId")]
    [QueryProperty(nameof(CurrentRemarks), "currentRemarks")]
    public class EditConsultationRemarksViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;
        private readonly IAppointmentService _appointmentService;

        private string _appointmentId;
        private string _currentRemarks;
        private string _editedRemarks;
        private string _patientName;
        private string _patientIC;
        private string _serviceType;
        private string _consultationDate;
        private ObservableCollection<ConsultationEditHistoryDto> _editHistory = new();

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

        public string CurrentRemarks
        {
            get => _currentRemarks;
            set
            {
                _currentRemarks = value;
                OnPropertyChanged();
                EditedRemarks = _currentRemarks;
            }
        }

        public string EditedRemarks
        {
            get => _editedRemarks;
            set => SetProperty(ref _editedRemarks, value);
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

        public string ConsultationDate
        {
            get => _consultationDate;
            set => SetProperty(ref _consultationDate, value);
        }

        public ObservableCollection<ConsultationEditHistoryDto> EditHistory
        {
            get => _editHistory;
            set => SetProperty(ref _editHistory, value);
        }

        public bool HasEditHistory => EditHistory?.Count > 0;

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }

        public EditConsultationRemarksViewModel(IAuthService authService, IConsultationService consultationService, IAppointmentService appointmentService)
        {
            _authService = authService;
            _consultationService = consultationService;
            _appointmentService = appointmentService;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            SaveCommand = new Command(async () => await SaveRemarks());
        }

        private async Task LoadAppointmentDetails()
        {
            try
            {
                if (string.IsNullOrEmpty(AppointmentId))
                    return;

                var appointment = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
                if (appointment != null)
                {
                    PatientName = appointment.Patient?.patient_name ?? "Unknown Patient";
                    PatientIC = appointment.patient_IC ?? "Unknown IC";
                    ServiceType = appointment.service_type?.ToString() ?? "General Consultation";
                    ConsultationDate = appointment.appointedAt?.ToString("dd MMM yyyy HH:mm") ?? "Unknown Date";

                    // Load consultation details
                    var consultation = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(AppointmentId);
                    if (consultation != null)
                    {
                        if (string.IsNullOrEmpty(CurrentRemarks))
                        {
                            CurrentRemarks = consultation.DoctorRemark ?? "No remarks available";
                        }

                        // Load edit history (if you implement this feature)
                        await LoadEditHistory();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading appointment details: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load appointment details", "OK");
            }
        }

        private async Task LoadEditHistory()
        {
            try
            {
                // TODO: Implement edit history loading from consultation service
                // For now, we'll leave it empty
                EditHistory.Clear();
                OnPropertyChanged(nameof(HasEditHistory));
            }
            catch (Exception)
            {
                // Silently ignore or log without variable if not used
            }
        }

        private async Task SaveRemarks()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditedRemarks))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter consultation remarks", "OK");
                    return;
                }

                if (EditedRemarks == CurrentRemarks)
                {
                    await Shell.Current.DisplayAlert("Info", "No changes made", "OK");
                    return;
                }

                // Save the updated remarks
                await _consultationService.UpdateConsultationRemarksAsync(AppointmentId, EditedRemarks);

                // Record edit history (if you implement this feature)
                await RecordEditHistory();

                await Shell.Current.DisplayAlert("Success", "Consultation remarks updated successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving remarks: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to save remarks", "OK");
            }
        }

        private async Task RecordEditHistory()
        {
            try
            {
                var currentUser = _authService.GetCurrentUser();
                if (currentUser != null)
                {
                    var editRecord = new ConsultationEditHistoryDto
                    {
                        AppointmentId = AppointmentId,
                        EditedDate = DateTime.Now,
                        EditedBy = currentUser.staff_name,
                        PreviousRemarks = CurrentRemarks,
                        NewRemarks = EditedRemarks
                    };

                    // TODO: Save edit history to database
                    // await _consultationService.SaveEditHistoryAsync(editRecord);
                }
            }
            catch (Exception)
            {
                // Unused ex
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

    public class ConsultationEditHistoryDto
    {
        public string AppointmentId { get; set; }
        public DateTime EditedDate { get; set; }
        public string EditedBy { get; set; }
        public string PreviousRemarks { get; set; }
        public string NewRemarks { get; set; }
    }
}