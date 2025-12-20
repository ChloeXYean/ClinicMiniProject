using System;
using System.Collections.ObjectModel;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace ClinicMiniProject.ViewModels
{
    public class ConsultationDetailsViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;

        private string _nurseRemark = string.Empty;
        private string _doctorRemark = string.Empty;

        private string _searchText = string.Empty;
        private ConsultationDetailsDto? _currentDetails;
        private string _appointmentId = string.Empty;
        private bool _isConsultationStarted = false;

        public bool IsNurse
        {
            get
            {
                var user = _authService.GetCurrentUser();
                //Doctor = false to know if it is nurse
                return user != null && !user.isDoctor;
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsDoctor => !IsNurse;

        public ConsultationDetailsDto? CurrentDetails
        {
            get => _currentDetails;
            set
            {
                if (SetProperty(ref _currentDetails, value))
                {
                    // Update the editable fields when data loads
                    if (value != null)
                    {
                        DoctorRemark = value.DoctorRemark ?? ""; 
                        NurseRemark = value.NurseRemark ?? "";      
                    }
                    OnPropertyChanged(nameof(PatientNameText));
                    OnPropertyChanged(nameof(PatientIcText));
                    OnPropertyChanged(nameof(AppointedTimeSlotText));
                    OnPropertyChanged(nameof(SelectedServiceTypeText));
                }
            }
        }

        public string AppointmentId
        {
            get => _appointmentId;
            set => SetProperty(ref _appointmentId, value);
        }

        public string DoctorRemark
        {
            get => _doctorRemark;
            set => SetProperty(ref _doctorRemark, value);
        }

        public string NurseRemark
        {
            get => _nurseRemark;
            set => SetProperty(ref _nurseRemark, value);
        }

        public bool IsConsultationStarted
        {
            get => _isConsultationStarted;
            set
            {
                if (SetProperty(ref _isConsultationStarted, value))
                {
                    OnPropertyChanged(nameof(IsStartButtonVisible));
                    OnPropertyChanged(nameof(IsRemarkBoxVisible));
                }
            }
        }

        // Only show "Start" if Doctor AND not started
        public bool IsStartButtonVisible => IsDoctor && !IsConsultationStarted;

        // Show Doctor Remark box if Started OR if we are just viewing details
        public bool IsRemarkBoxVisible => IsConsultationStarted || (CurrentDetails != null && CurrentDetails.Status == "Completed");

        public string DateText => CurrentDetails == null ? "" : $"Date: {CurrentDetails.Date:dd MMMM yyyy}";
        public string AppointedTimeSlotText => CurrentDetails == null ? "" : $"Appointed Time Slot: {CurrentDetails.AppointedTimeSlot:h:mm tt}";
        public string PatientNameText => CurrentDetails == null ? "" : $"Patient Name: {CurrentDetails.PatientName}";
        public string PatientIcText => CurrentDetails == null ? "" : $"IC Number: {CurrentDetails.PatientIc}";
        public string PatientPhoneText => CurrentDetails == null ? "" : $"Phone Number: {CurrentDetails.PatientPhone}";
        public string SelectedServiceTypeText => CurrentDetails == null ? "" : $"Selected Service Type: {CurrentDetails.SelectedServiceType}";

        public ObservableCollection<PatientLookupDto> SearchResults { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand StartConsultationCommand { get; }
        public ICommand EndConsultationCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConsultationDetailsViewModel(IAuthService authService, IConsultationService consultationService)
        {
            _authService = authService;
            _consultationService = consultationService;

            RefreshCommand = new Command(async () => await RefreshAsync());
            SearchCommand = new Command(async () => await SearchAsync());
            StartConsultationCommand = new Command(async () => await StartAsync());
            EndConsultationCommand = new Command(async () => await EndAsync());
        }

        public async void LoadByAppointmentIdAsync()
        {
            if (string.IsNullOrWhiteSpace(AppointmentId)) return;
            CurrentDetails = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(AppointmentId);

  
            IsConsultationStarted = true;
            
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("AppointmentId"))
            {
                AppointmentId = query["AppointmentId"].ToString();
                LoadByAppointmentIdAsync();
            }
        }

        public async Task RefreshAsync()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

            CurrentDetails = await _consultationService.GetCurrentConsultationDetailsAsync(doctor.staff_ID, DateTime.Now);
        }

        public async Task SearchAsync()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            var results = await _consultationService.SearchPatientsByNameAsync(SearchText);
            foreach (var r in results)
                SearchResults.Add(r);
        }

        public async Task StartAsync()
        {
            if (string.IsNullOrWhiteSpace(AppointmentId)) return;
            await _consultationService.StartConsultationAsync(AppointmentId);
            IsConsultationStarted = true;
        }

        public async Task EndAsync()
        {
            if (string.IsNullOrWhiteSpace(AppointmentId)) return;

            bool confirm = await Shell.Current.DisplayAlert("Finish?", "End this consultation?", "Yes", "No");
            if (!confirm) return;

            // Save Doctor Remark (and preserve Nurse remark)
            await _consultationService.EndConsultationAsync(AppointmentId, DoctorRemark, NurseRemark);

            await Shell.Current.GoToAsync("..");
        }


        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
