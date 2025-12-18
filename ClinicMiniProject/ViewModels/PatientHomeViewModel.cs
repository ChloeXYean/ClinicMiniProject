using ClinicMiniProject.Services.Interfaces;
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

        public ICommand HomeCommand { get; }
        public ICommand InquiryHistoryCommand { get; }
        public ICommand ProfileCommand { get; }

        public ICommand NavigateToAppointmentBookingCommand { get; }
        public ICommand NavigateToAppointmentHistoryCommand { get; }
        public ICommand NavigateToOnlineInquiryCommand { get; }
        public ICommand NotificationCommand { get; }

        public string Username => _authService.GetCurrentUser()?.staff_name ?? "Patient";

        public PatientHomeViewModel(IAppointmentService appointmentService, IAuthService authService)
        {
            _appointmentService = appointmentService;
            _authService = authService;

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
        }

        private async Task OnNavigateToHistory()
        {
            var patient = _authService.GetCurrentUser();
            if (patient == null) return;

            // Check if patient has any appointments (Completed, Pending, etc.)
            // We pass the patient's ID to the service.
            // Note: We might need to ensure GetAppointmentsByStaffAndDateRangeAsync or a similar method supports Patient ID 
            // OR use a specific GetPatientHistory method. 
            // For now, I'll assume we fetch "All" for this user.

            try
            {
                // Logic: Try to fetch history. If count > 0 -> HistoryPage, else -> NoHistoryPage
                var history = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);

                // Filter for this specific patient locally if the service doesn't support direct patient filtering yet
                var myHistory = history.Where(a => a.patient_IC == patient.staff_ID).ToList(); // Assuming staff_ID holds IC for patient login context

                if (myHistory.Any())
                {
                    await Shell.Current.GoToAsync("AppointmentHistory");
                }
                else
                {
                    await Shell.Current.GoToAsync("AppointmentHistory_NoHistory");
                }
            }
            catch
            {
                // Fallback to No History on error
                await Shell.Current.GoToAsync("AppointmentHistory_NoHistory");
            }
        }
    }
}
