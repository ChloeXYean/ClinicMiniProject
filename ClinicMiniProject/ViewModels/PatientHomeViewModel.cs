using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Services;
using ClinicMiniProject.UI.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class PatientHomeViewModel : BindableObject
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthService _authService;
        private readonly PatientService _patientService;

        public ICommand HomeCommand { get; }
        public ICommand InquiryHistoryCommand { get; }
        public ICommand ProfileCommand { get; }

        public ICommand NavigateToAppointmentBookingCommand { get; }
        public ICommand NavigateToAppointmentHistoryCommand { get; }
        public ICommand NavigateToOnlineInquiryCommand { get; }
        public ICommand NotificationCommand { get; }

        private bool _hasUpcomingAppointment;
        public bool HasUpcomingAppointment
        {
            get => _hasUpcomingAppointment;
            set { _hasUpcomingAppointment = value; OnPropertyChanged(); }
        }

        private string _appointmentDate;
        public string AppointmentDate
        {
            get => _appointmentDate;
            set { _appointmentDate = value; OnPropertyChanged(); }
        }

        private string _appointmentTime;
        public string AppointmentTime
        {
            get => _appointmentTime;
            set { _appointmentTime = value; OnPropertyChanged(); }
        }

        private string _doctorName;
        public string DoctorName
        {
            get => _doctorName;
            set { _doctorName = value; OnPropertyChanged(); }
        }

        private string _queueSequence;
        public string QueueSequence
        {
            get => _queueSequence;
            set { _queueSequence = value; OnPropertyChanged(); }
        }

        public string Username => _authService.GetCurrentPatient()?.patient_name ?? _authService.GetCurrentUser()?.staff_name ?? "Patient";

        public PatientHomeViewModel(IAppointmentService appointmentService, IAuthService authService, PatientService patientService)
        {
            _appointmentService = appointmentService;
            _authService = authService;
            _patientService = patientService;

            HomeCommand = new Command((async () => await Shell.Current.GoToAsync("")));

            // Patient -> Inquiry History
            InquiryHistoryCommand = new Command(async () =>
                await Shell.Current.GoToAsync("InquiryHistory"));

            // Patient -> Patient Details (Shared Page)
            ProfileCommand = new Command(async () =>
                await Shell.Current.GoToAsync("PatientDetails"));


            // 2. Main Page Buttons
            NavigateToAppointmentBookingCommand = new Command(async () =>
                await Shell.Current.GoToAsync("AppointmentBooking")); // Ensure this route exists or is AppointmentBooking_Patient

            NavigateToAppointmentHistoryCommand = new Command(async () => await OnNavigateToHistory());

            NavigateToOnlineInquiryCommand = new Command(async () =>
                await Shell.Current.GoToAsync("OnlineInquiry"));

            NotificationCommand = new Command(async () =>
                await Shell.Current.DisplayAlert("Notification", "You have no new notifications.", "OK"));

            _ = LoadUpcomingAppointment();
        }

        private async Task LoadUpcomingAppointment()
        {
            var patient = _authService.GetCurrentPatient();
            if (patient == null)
            {
                System.Diagnostics.Debug.WriteLine("[PatientHomeViewModel] No current patient found in AuthService.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[PatientHomeViewModel] Loading upcoming appointment for patient: {patient.patient_name} (IC: {patient.patient_IC})");

            try
            {
                var upcoming = await _patientService.GetUpcomingAppointmentEntityAsync(patient.patient_IC);
                if (upcoming != null)
                {
                    HasUpcomingAppointment = true;
                    AppointmentDate = upcoming.appointedAt?.ToString("dd MMM yyyy") ?? "N/A";
                    AppointmentTime = upcoming.appointedAt?.ToString("HH:mm") ?? "N/A";
                    DoctorName = upcoming.Staff?.staff_name ?? "Doctor";

                    int ahead = await _patientService.GetConsultationQueueAsync(upcoming);
                    QueueSequence = ahead.ToString();

                    System.Diagnostics.Debug.WriteLine($"[PatientHomeViewModel] Loaded appointment: {AppointmentDate} at {AppointmentTime} with {DoctorName}");
                }
                else
                {
                    HasUpcomingAppointment = false;
                    System.Diagnostics.Debug.WriteLine("[PatientHomeViewModel] No upcoming appointment returned from service.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientHomeViewModel] Error loading upcoming appointment: {ex.Message}");
                HasUpcomingAppointment = false;
            }
        }

        public void ReloadData()
        {
            OnPropertyChanged(nameof(Username));
            _ = LoadUpcomingAppointment();
        }

        private async Task OnNavigateToHistory()
        {
            var patient = _authService.GetCurrentPatient();
            var staff = _authService.GetCurrentUser();
            string userIc = patient?.patient_IC ?? staff?.staff_ID;

            if (string.IsNullOrEmpty(userIc)) return;

            try
            {
                var history = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);
                var myHistory = history.Where(a => a.patient_IC == userIc).ToList();

                await Shell.Current.GoToAsync("PatientAppointmentHistory?UserType=Patient");
            }
            catch
            {
                await Shell.Current.GoToAsync("PatientAppointmentHistory?UserType=Patient");
            }
        }
    }
}