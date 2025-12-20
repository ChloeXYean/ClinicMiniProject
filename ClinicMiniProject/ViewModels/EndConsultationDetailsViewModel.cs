using ClinicMiniProject.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class EndConsultationDetailsViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly IConsultationService _consultationService;
        private readonly IAuthService _authService;

        private string _appointmentId;
        private string _patientName;
        private string _patientIC;
        private string _serviceType;
        private string _doctorRemark;
        private string _nurseRemark;

        public string PatientName { get => _patientName; set => SetProperty(ref _patientName, value); }
        public string PatientIC { get => _patientIC; set => SetProperty(ref _patientIC, value); }
        public string ServiceType { get => _serviceType; set => SetProperty(ref _serviceType, value); }

        public string DoctorRemark { get => _doctorRemark; set => SetProperty(ref _doctorRemark, value); }

        public string NurseRemark { get => _nurseRemark; set => SetProperty(ref _nurseRemark, value); }

        public ICommand EndConsultationCommand { get; }
        public bool IsDoctor
        {
            get
            {
                var user = _authService.GetCurrentUser();
                return user != null && user.isDoctor;
            }
        }

        public bool IsNurse => !IsDoctor;


        public EndConsultationDetailsViewModel(IConsultationService consultationService, IAuthService authService)
        {
            _consultationService = consultationService;
            _authService = authService; 
            EndConsultationCommand = new Command(async () => await EndAsync());
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("AppointmentId"))
            {
                _appointmentId = query["AppointmentId"].ToString();
                LoadDetails();
            }
        }

        private async void LoadDetails()
        {
            if (string.IsNullOrEmpty(_appointmentId)) return;

            var details = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(_appointmentId);
            if (details != null)
            {
                PatientName = details.PatientName;
                PatientIC = details.PatientIc;
                ServiceType = details.SelectedServiceType ?? "General Consultation";
                NurseRemark = details.NurseRemark;
                DoctorRemark = details.DoctorRemark;
                OnPropertyChanged(nameof(IsDoctor));
                OnPropertyChanged(nameof(IsNurse));

            }
        }

        // 3. Save and End
        private async Task EndAsync()
        {
            if (string.IsNullOrEmpty(_appointmentId)) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm", "End this consultation?", "Yes", "No");
            if (!confirm) return;

            await _consultationService.EndConsultationAsync(_appointmentId, DoctorRemark ?? "", NurseRemark);

            await Shell.Current.DisplayAlert("Success", "Consultation ended successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }


        #region Property Changed Helpers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
        #endregion
    }
}