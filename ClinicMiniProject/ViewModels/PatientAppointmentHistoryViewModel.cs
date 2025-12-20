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
        private List<AppointmentHistoryItem> _originalAppointments; // Store full list for filtering

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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterAppointments();
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
                _ = LoadAppointments();
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
            _originalAppointments = new List<AppointmentHistoryItem>();

            LoadAppointmentsCommand = new Command(async () => await LoadAppointments());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Auto load on init
            _ = LoadAppointments();
        }

        private async Task LoadAppointments()
        {
            try
            {
                List<Appointment> rawData = new();

                // ---------------------------------------------------------
                // 1. DATA FETCHING (Background)
                // ---------------------------------------------------------
                if (UserType == "Nurse")
                {
                    rawData = await _nurseController.GetAllAppointmentsHistory();
                }
                else if (UserType == "Doctor")
                {
                    var staff = _authService.GetCurrentUser();
                    if (staff != null)
                    {
                        var allApps = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(staff.staff_ID, DateTime.MinValue, DateTime.MaxValue);
                        rawData = allApps.ToList();
                    }
                }
                else
                {
                    var user = _authService.GetCurrentPatient();
                    if (user != null)
                    {
                        var allApps = await _appointmentService.GetAppointmentsByPatientIcAsync(user.patient_IC);
                        rawData = allApps.ToList();
                    }
                }

                // ---------------------------------------------------------
                // 2. MAPPING TO UI ITEMS
                // ---------------------------------------------------------
                var uiItems = new List<AppointmentHistoryItem>();

                foreach (var appt in rawData)
                {
                    string status = appt.status ?? "Pending";
                    var (cardBg, badgeBg) = GetColors(status);

                    string nameDisplay;
                    string doctorInternalName;
                    if (UserType == "Patient")
                    {
                        nameDisplay = $"Dr. {(appt.Staff != null ? appt.Staff.staff_name : "Unknown")}";
                        doctorInternalName = appt.Staff?.staff_name ?? "Unknown";
                    }
                    else
                    {
                        var p = _nurseController.ViewPatientDetails(appt.patient_IC);
                        string realName = p != null ? p.patient_name : "Unknown";
                        nameDisplay = $"Patient: {realName}";
                        doctorInternalName = realName;
                    }

                    string timeDisplay = appt.appointedAt?.ToString("hh:mm tt") + " - " + appt.appointedAt?.AddHours(1).ToString("hh:mm tt");

                    uiItems.Add(new AppointmentHistoryItem
                    {
                        Time = timeDisplay,
                        Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "-",
                        RawDate = appt.appointedAt ?? DateTime.MinValue,
                        Details = $"{nameDisplay} (#{appt.appointment_ID})",
                        DoctorName = (UserType == "Patient") ? doctorInternalName : nameDisplay,
                        Status = status,
                        StatusColor = Colors.Black,
                        BadgeColor = badgeBg,
                        CardBackgroundColor = cardBg
                    });
                }

                // ---------------------------------------------------------
                // 3. UI UPDATE
                // ---------------------------------------------------------
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _originalAppointments = uiItems;
                    FilterAppointments();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
            }
        }

        private void FilterAppointments()
        {
            if (_originalAppointments == null) return;

            var filtered = _originalAppointments.AsEnumerable();

            // 1. FILTER BY SEARCH TEXT
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.ToLower();
                filtered = filtered.Where(a => 
                    (a.DoctorName != null && a.DoctorName.ToLower().Contains(search)) ||
                    (a.Details != null && a.Details.ToLower().Contains(search)));
            }

            // 2. SORTING: Pending (0) -> Completed (1) -> Cancelled (2)
            // Then sort by latest date first
            var sorted = filtered
                .OrderBy(a => a.Status?.ToLower() switch
                {
                    "pending" => 0,
                    "completed" => 1,
                    "cancelled" => 2,
                    _ => 3
                })
                .ThenBy(a => a.RawDate)
                .ToList();

            Appointments.Clear();
            foreach (var item in sorted)
            {
                Appointments.Add(item);
            }

            // Sync visibility properties
            OnPropertyChanged(nameof(HasNoAppointments));
            OnPropertyChanged(nameof(HasAppointments));
        }

        private (Color CardBg, Color BadgeBg) GetColors(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => (Color.FromArgb("#E3F2FD"), Color.FromArgb("#42A5F5")),   // Blue
                "cancelled" => (Color.FromArgb("#FFEBEE"), Color.FromArgb("#EF5350")), // Red
                "completed" => (Color.FromArgb("#FFFDE7"), Color.FromArgb("#FFCA28")), // Yellow
                _ => (Colors.White, Colors.Gray)
            };
        }
    }

    public class AppointmentHistoryItem
    {
        public string Time { get; set; }
        public string Date { get; set; }
        public DateTime RawDate { get; set; } // Added for sorting
        public string Details { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
        public Color StatusColor { get; set; } // Text Color (e.g. Black or specific)
        public Color BadgeColor { get; set; } // Background of the status pill
        public Color CardBackgroundColor { get; set; } // Background of the entire card
    }
}