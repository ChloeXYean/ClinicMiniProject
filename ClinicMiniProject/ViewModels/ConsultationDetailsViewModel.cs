using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public class ConsultationDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;

        private string _searchText = string.Empty;
        private ConsultationDetailsDto? _currentDetails;
        private string _appointmentId = string.Empty;
        private string _consultationRemark = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ConsultationDetailsDto? CurrentDetails
        {
            get => _currentDetails;
            set
            {
                if (SetProperty(ref _currentDetails, value))
                {
                    OnPropertyChanged(nameof(DateText));
                    OnPropertyChanged(nameof(AppointedTimeSlotText));
                    OnPropertyChanged(nameof(PatientNameText));
                    OnPropertyChanged(nameof(PatientIcText));
                    OnPropertyChanged(nameof(PatientPhoneText));
                    OnPropertyChanged(nameof(SelectedServiceTypeText));
                }
            }
        }

        public string AppointmentId
        {
            get => _appointmentId;
            set
            {
                if (SetProperty(ref _appointmentId, value))
                {
                    _ = LoadByAppointmentIdAsync();
                }
            }
        }

        public string ConsultationRemark
        {
            get => _consultationRemark;
            set => SetProperty(ref _consultationRemark, value);
        }

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

        public async Task LoadByAppointmentIdAsync()
        {
            if (string.IsNullOrWhiteSpace(AppointmentId))
                return;

            CurrentDetails = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(AppointmentId);
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
            if (CurrentDetails == null)
                return;

            await _consultationService.StartConsultationAsync(CurrentDetails.AppointmentId);
            await RefreshAsync();
        }

        public async Task EndAsync()
        {
            var apptId = CurrentDetails?.AppointmentId;
            if (string.IsNullOrWhiteSpace(apptId))
                apptId = AppointmentId;

            if (string.IsNullOrWhiteSpace(apptId))
                return;

            await _consultationService.EndConsultationAsync(apptId, ConsultationRemark);
            await Shell.Current.GoToAsync("..");
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

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
