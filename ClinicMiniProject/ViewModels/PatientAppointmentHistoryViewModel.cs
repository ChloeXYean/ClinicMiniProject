using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.ViewModels
{
    public class PatientAppointmentHistoryViewModel : BindableObject
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthService _authService;
        private ObservableCollection<AppointmentHistoryItem> _appointments;

        public ObservableCollection<AppointmentHistoryItem> Appointments
        {
            get => _appointments;
            set
            {
                _appointments = value;
                OnPropertyChanged();
                
                // Update visibility properties
                OnPropertyChanged(nameof(HasNoAppointments));
                OnPropertyChanged(nameof(HasAppointments));
            }
        }

        public bool HasNoAppointments => Appointments == null || Appointments.Count == 0;
        public bool HasAppointments => Appointments != null && Appointments.Count > 0;

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand BackCommand { get; }

        public PatientAppointmentHistoryViewModel(IAppointmentService appointmentService, IAuthService authService)
        {
            _appointmentService = appointmentService;
            _authService = authService;
            Appointments = new ObservableCollection<AppointmentHistoryItem>();

            LoadAppointmentsCommand = new Command(async () => await LoadAppointments());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Auto load on init
            Task.Run(LoadAppointments);
        }

        private async Task LoadAppointments()
        {
            var user = _authService.GetCurrentPatient(); // Getting logged in patient
            // Fallback to staff if null (for testing) or handle generic user
            string userIc = user?.patient_IC ?? _authService.GetCurrentUser()?.staff_ID;

            if (string.IsNullOrEmpty(userIc)) return;

            try
            {
                // Fetch all appointments (or filter by patient logic if available in service)
                var allAppointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);

                // Filter by patient ID
                var myAppointments = allAppointments
                    .Where(a => a.patient_IC == userIc)
                    .OrderByDescending(a => a.appointedAt)
                    .ToList();

                var items = new List<AppointmentHistoryItem>();
                foreach (var appt in myAppointments)
                {
                    string status = appt.status ?? "Pending";
                    var (cardBg, badgeBg) = GetColors(status);

                    items.Add(new AppointmentHistoryItem
                    {
                        Time = appt.appointedAt?.ToString("hh:mm tt") + " - " + appt.appointedAt?.AddHours(1).ToString("hh:mm tt"), // Assuming 1h duration
                        Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "",
                        Details = $"Clinic Visit (#{appt.appointment_ID})", // Example format
                        DoctorName = appt.Staff != null ? appt.Staff.staff_name : "Dr. Unknown",
                        Status = status,
                        StatusColor = Colors.Black, // Text color for badge
                        BadgeColor = badgeBg,
                        CardBackgroundColor = cardBg
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Appointments.Clear();
                    foreach (var item in items)
                    {
                        Appointments.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
            }
        }

        private (Color CardBg, Color BadgeBg) GetColors(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => (Color.FromArgb("#E8F5E9"), Color.FromArgb("#4CAF50")), // Green
                "cancelled" => (Color.FromArgb("#FFEBEE"), Color.FromArgb("#EF5350")), // Red
                "pending" => (Color.FromArgb("#FFFDE7"), Color.FromArgb("#FFCA28")),   // Yellow
                "scheduled" => (Color.FromArgb("#E3F2FD"), Color.FromArgb("#42A5F5")), // Blue (Adding scheduled as blue)
                _ => (Colors.White, Colors.Gray)
            };
        }
    }

    public class AppointmentHistoryItem
    {
        public string Time { get; set; }
        public string Date { get; set; }
        public string Details { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
        public Color StatusColor { get; set; } // Text Color (e.g. Black or specific)
        public Color BadgeColor { get; set; } // Background of the status pill
        public Color CardBackgroundColor { get; set; } // Background of the entire card
    }
}