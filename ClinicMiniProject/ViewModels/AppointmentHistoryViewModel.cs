using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Controller;     
using ClinicMiniProject.Services.Interfaces; 
using ClinicMiniProject.Models;
using ClinicMiniProject.Dtos;         
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(UserType), "UserType")]
    public class AppointmentHistoryViewModel : BindableObject
    {
        private readonly NurseController _nurseController;
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
                OnPropertyChanged(nameof(HasNoAppointments));
                OnPropertyChanged(nameof(HasAppointments));
            }
        }

        private string userType;
        public string UserType
        {
            get => userType;
            set
            {
                userType = value;
                OnPropertyChanged();
                Task.Run(LoadAppointments);
            }
        }

        public bool HasNoAppointments => Appointments == null || Appointments.Count == 0;
        public bool HasAppointments => Appointments != null && Appointments.Count > 0;

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand BackCommand { get; }

        public AppointmentHistoryViewModel(
            NurseController nurseController,
            IAppointmentService appointmentService,
            IAuthService authService)
        {
            _nurseController = nurseController;
            _appointmentService = appointmentService;
            _authService = authService;

            Appointments = new ObservableCollection<AppointmentHistoryItem>();

            LoadAppointmentsCommand = new Command(async () => await LoadAppointments());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task LoadAppointments()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    List<Appointment> rawData = new();

                    if (UserType == "Nurse")
                    {
                        // --- NURSE: Get ALL History ---
                        rawData = await _nurseController.GetAllAppointmentsHistory();
                    }
                    else
                    {
                        // --- PATIENT: Get ONLY My History ---
                        var user = _authService.GetCurrentPatient();
                        string userIc = user?.patient_IC;

                        if (!string.IsNullOrEmpty(userIc))
                        {
                            var allApps = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);

                            rawData = allApps
                                .Where(a => a.patient_IC == userIc)
                                .OrderByDescending(a => a.appointedAt)
                                .ToList();
                        }
                    }

                    var uiItems = new List<AppointmentHistoryItem>();

                    foreach (var appt in rawData)
                    {
                        string status = appt.status ?? "Pending";
                        var (cardBg, badgeBg) = GetColors(status);

                        var patient = _nurseController.ViewPatientDetails(appt.patient_IC);
                        string realName = patient != null ? patient.patient_name : "Unknown Name";
                        string nameDisplay = (UserType == "Nurse")
                            ? $"Patient: {realName}" 
                            : $"Dr. {(appt.Staff != null ? appt.Staff.staff_name : "Unknown")}";

                        uiItems.Add(new AppointmentHistoryItem
                        {
                            Time = appt.appointedAt?.ToString("hh:mm tt") ?? "--",
                            Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "-",
                            Details = nameDisplay,
                            DoctorName = appt.service_type ?? "General Checkup", // Reuse this field for Service Type
                            Status = status,
                            StatusColor = Colors.Black,
                            BadgeColor = badgeBg,
                            CardBackgroundColor = cardBg
                        });
                    }

                    // 6. UPDATE UI
                    Appointments.Clear();
                    foreach (var item in uiItems)
                    {
                        Appointments.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
                }
            });
        }

        private (Color CardBg, Color BadgeBg) GetColors(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => (Color.FromArgb("#E8F5E9"), Color.FromArgb("#4CAF50")), // Green
                "cancelled" => (Color.FromArgb("#FFEBEE"), Color.FromArgb("#EF5350")), // Red
                "pending" => (Color.FromArgb("#FFFDE7"), Color.FromArgb("#FFCA28")), // Yellow
                "scheduled" => (Color.FromArgb("#E3F2FD"), Color.FromArgb("#42A5F5")), // Blue
                _ => (Colors.White, Colors.Gray)
            };
        }
    }
}