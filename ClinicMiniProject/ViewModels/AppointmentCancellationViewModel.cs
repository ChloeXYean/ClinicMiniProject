using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(AppointmentId), "appointmentId")]
    public class AppointmentCancellationViewModel : BindableObject
    {
        private readonly IAppointmentService _appointmentService;
        private string _appointmentId;
        private string _date;
        private string _time;
        private string _doctorName;
        private bool _isBusy;

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

        public string Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public string Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(); }
        }

        public string DoctorName
        {
            get => _doctorName;
            set { _doctorName = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand ConfirmCancelCommand { get; }
        public ICommand GoBackCommand { get; }

        public AppointmentCancellationViewModel(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
            ConfirmCancelCommand = new Command(async () => await OnConfirmCancel());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task LoadAppointmentDetails()
        {
            if (string.IsNullOrEmpty(AppointmentId)) return;

            IsBusy = true;
            try
            {
                // In a real app, we'd fetch the specific appointment by ID.
                // For simplicity here, we assume the data is passed or fetched.
                // Let's use the service to get it if available.
                // (Note: AppointmentService doesn't have GetByApptId yet, might need adjustment)
                var appointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);
                var appt = appointments.FirstOrDefault(a => a.appointment_ID == AppointmentId);

                if (appt != null)
                {
                    Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "N/A";
                    Time = appt.appointedAt?.ToString("hh:mm tt") ?? "N/A";
                    DoctorName = appt.Staff?.staff_name ?? "Doctor";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading appointment details: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnConfirmCancel()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var success = await _appointmentService.CancelAppointmentAsync(AppointmentId);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Appointment cancelled successfully.", "OK");
                    await Shell.Current.GoToAsync("///PatientHomePage");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to cancel appointment. Please try again.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
