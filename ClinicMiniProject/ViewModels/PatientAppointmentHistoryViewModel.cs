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
        private List<AppointmentHistoryItem> _originalAppointments;

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
                _ = LoadAppointments(); // Reload when UserType changes
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
            BackCommand = new Command(async () => await GoBack());
            // Initial load
            _ = LoadAppointments();
        }
        private async Task GoBack()
        {
            if (UserType == "Nurse")
            {
                await Shell.Current.GoToAsync("///NurseHomePage");
            }
            else
            {
                await Shell.Current.GoToAsync("///DoctorDashboardPage");
            }
        }

        private async Task LoadAppointments()
        {
            try
            {
                List<Appointment> rawData = new();
                bool isStaffView = false;

                // ---------------------------------------------------------
                // 1. DATA FETCHING (Preserving "Else = Patient" safety)
                // ---------------------------------------------------------
                if (UserType == "Nurse" || UserType == "Doctor")
                {
                    // --- STAFF LOGIC: SEE ALL HISTORY ---
                    isStaffView = true;
                    // Ensure getAll includes Patient/Staff details
                    rawData = await _nurseController.GetAllAppointmentsHistory();
                }
                else
                {
                    // --- PATIENT LOGIC (Default): SEE ONLY OWN HISTORY ---
                    isStaffView = false;
                    var user = _authService.GetCurrentPatient();
                    if (user != null)
                    {
                        var allApps = await _appointmentService.GetAppointmentsByPatientIcAsync(user.patient_IC);
                        rawData = allApps.ToList();
                    }
                }

                // ---------------------------------------------------------
                // 2. MAPPING TO UI
                // ---------------------------------------------------------
                var uiItems = new List<AppointmentHistoryItem>();

                foreach (var appt in rawData)
                {
                    string status = appt.status ?? "Pending";
                    var (cardBg, badgeBg) = GetColors(status);

                    string mainHeaderName;   // The bold text
                    string subDetailText;    // The smaller text

                    if (isStaffView)
                    {
                        // STAFF SEE: "Patient: Ali" (Dr. Steven)
                        string pName = appt.Patient?.patient_name
                                       ?? _nurseController.ViewPatientDetails(appt.patient_IC)?.patient_name
                                       ?? "Unknown";

                        string dName = appt.Staff?.staff_name ?? appt.staff_ID;

                        mainHeaderName = $"Patient: {pName}";
                        subDetailText = $"Dr. {dName} - {appt.service_type}";
                    }
                    else
                    {
                        // PATIENT SEES: "Dr. Steven" (General Consultation)
                        string dName = appt.Staff?.staff_name ?? "Unknown Doctor";

                        mainHeaderName = $"Dr. {dName}";
                        subDetailText = appt.service_type.ToString();
                    }

                    string timeDisplay = appt.appointedAt?.ToString("hh:mm tt") + " - " + appt.appointedAt?.AddHours(1).ToString("hh:mm tt");

                    uiItems.Add(new AppointmentHistoryItem
                    {
                        Time = timeDisplay,
                        Date = appt.appointedAt?.ToString("dd MMM yyyy") ?? "-",
                        RawDate = appt.appointedAt ?? DateTime.MinValue,

                        // IMPORTANT: DoctorName property is used for the Main Header in your XAML
                        DoctorName = mainHeaderName,
                        Details = subDetailText,

                        Status = status,
                        StatusColor = Colors.Black,
                        BadgeColor = badgeBg,
                        CardBackgroundColor = cardBg,
                        AppointmentId = appt.appointment_ID,
                        ViewDetailsCommand = new Command(async () => await NavigateToConsultationDetails(appt.appointment_ID))
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

            // 1. FILTER
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.ToLower();
                filtered = filtered.Where(a =>
                    (a.DoctorName != null && a.DoctorName.ToLower().Contains(search)) ||
                    (a.Details != null && a.Details.ToLower().Contains(search)));
            }

            // 2. SORTING: Status Priority, then Nearest Date First
            var sorted = filtered
                .OrderBy(a => a.Status?.ToLower() switch
                {
                    "pending" => 0,
                    "completed" => 1,
                    "cancelled" => 2,
                    _ => 3
                })
                .ThenBy(a => a.RawDate) // Nearest date first within each status
                .ToList();

            Appointments.Clear();
            foreach (var item in sorted)
            {
                Appointments.Add(item);
            }

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

        private async Task NavigateToConsultationDetails(string appointmentId)
        {
            if (!string.IsNullOrEmpty(appointmentId))
            {
                await Shell.Current.GoToAsync($"///ConsultationDetailsPage?appointmentId={appointmentId}");
            }
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
        public string AppointmentId { get; set; }
        public ICommand ViewDetailsCommand { get; set; }
    }
}