using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Models;
using ClinicMiniProject.Controller;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(UserType), "UserType")]

    public class PatientAppointmentHistoryViewModel : BindableObject
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
                
                // Update visibility properties
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

        public PatientAppointmentHistoryViewModel(NurseController nurseController, IAppointmentService appointmentService, IAuthService authService)
        {
            _nurseController = nurseController;
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
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    List<Appointment> rawData = new();

                    // ---------------------------------------------------------
                    // 1. NURSE LOGIC (From Upper Code)
                    // ---------------------------------------------------------
                    if (UserType == "Nurse")
                    {
                        // Nurse sees EVERYTHING
                        rawData = await _nurseController.GetAllAppointmentsHistory();
                    }
                    // ---------------------------------------------------------
                    // 2. DOCTOR LOGIC (Explicit handling for Staff ID)
                    // ---------------------------------------------------------
                    else if (UserType == "Doctor")
                    {
                        var staff = _authService.GetCurrentUser();
                        if (staff != null)
                        {
                            // Fetch only appointments assigned to this Doctor
                            var allApps = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(staff.staff_ID, DateTime.MinValue, DateTime.MaxValue);
                            rawData = allApps.OrderByDescending(a => a.appointedAt).ToList();
                        }
                    }
                    // ---------------------------------------------------------
                    // 3. PATIENT LOGIC (From Lower Code + Upper Code Logic)
                    // ---------------------------------------------------------
                    else
                    {
                        // Logic from Bottom Code: Try getting Patient, fallback to Staff ID if needed (for testing)
                        var user = _authService.GetCurrentPatient();
                        string userIc = user?.patient_IC ?? _authService.GetCurrentUser()?.staff_ID;

                        if (!string.IsNullOrEmpty(userIc))
                        {
                            var allApps = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);

                            rawData = allApps
                                .Where(a => a.patient_IC == userIc)
                                .OrderByDescending(a => a.appointedAt)
                                .ToList();
                        }
                    }

                    // ---------------------------------------------------------
                    // 4. MAPPING TO UI (Combining Best Features)
                    // ---------------------------------------------------------
                    var uiItems = new List<AppointmentHistoryItem>();

                    foreach (var appt in rawData)
                    {
                        string status = appt.status ?? "Pending";
                        var (cardBg, badgeBg) = GetColors(status);

                        // LOGIC: "Who am I looking at?"
                        // If I am a Patient -> Show me the Doctor's Name
                        // If I am a Nurse/Doctor -> Show me the Patient's Name
                        string nameDisplay;
                        if (UserType == "Patient")
                        {
                            nameDisplay = $"Dr. {(appt.Staff != null ? appt.Staff.staff_name : "Unknown")}";
                        }
                        else
                        {
                            // Use NurseController to fetch real patient name if possible
                            var p = _nurseController.ViewPatientDetails(appt.patient_IC);
                            string realName = p != null ? p.patient_name : "Unknown";
                            nameDisplay = $"Patient: {realName}";
                        }

                        // TIME FORMAT: Using the "Start - End" format from your Bottom Code
                        string timeDisplay = appt.appointedAt?.ToString("hh:mm tt") + " - " + appt.appointedAt?.AddHours(1).ToString("hh:mm tt");

                        uiItems.Add(new AppointmentHistoryItem
                        {
                            Time = timeDisplay,
                            Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "-",

                            // Display the Name (Dr. or Patient) + ID
                            Details = $"{nameDisplay} (#{appt.appointment_ID})",

                            DoctorName = appt.service_type ?? "General Checkup",
                            Status = status,
                            StatusColor = Colors.Black,
                            BadgeColor = badgeBg,
                            CardBackgroundColor = cardBg
                        });
                    }

                    // ---------------------------------------------------------
                    // 5. UPDATE UI
                    // ---------------------------------------------------------
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

        //private async Task LoadAppointments()
        //{
        //    var user = _authService.GetCurrentPatient(); // Getting logged in patient
        //    // Fallback to staff if null (for testing) or handle generic user
        //    string userIc = user?.patient_IC ?? _authService.GetCurrentUser()?.staff_ID;

        //    if (string.IsNullOrEmpty(userIc)) return;

        //    try
        //    {
        //        // Fetch all appointments (or filter by patient logic if available in service)
        //        var allAppointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(null, DateTime.MinValue, DateTime.MaxValue);

        //        // Filter by patient ID
        //        var myAppointments = allAppointments
        //            .Where(a => a.patient_IC == userIc)
        //            .OrderByDescending(a => a.appointedAt)
        //            .ToList();

        //        var items = new List<AppointmentHistoryItem>();
        //        foreach (var appt in myAppointments)
        //        {
        //            string status = appt.status ?? "Pending";
        //            var (cardBg, badgeBg) = GetColors(status);

        //            items.Add(new AppointmentHistoryItem
        //            {
        //                Time = appt.appointedAt?.ToString("hh:mm tt") + " - " + appt.appointedAt?.AddHours(1).ToString("hh:mm tt"), // Assuming 1h duration
        //                Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "",
        //                Details = $"Clinic Visit (#{appt.appointment_ID})", // Example format
        //                DoctorName = appt.Staff != null ? appt.Staff.staff_name : "Dr. Unknown",
        //                Status = status,
        //                StatusColor = Colors.Black, // Text color for badge
        //                BadgeColor = badgeBg,
        //                CardBackgroundColor = cardBg
        //            });
        //        }

        //        MainThread.BeginInvokeOnMainThread(() =>
        //        {
        //            Appointments.Clear();
        //            foreach (var item in items)
        //            {
        //                Appointments.Add(item);
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
        //    }
        //}

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