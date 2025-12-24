
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
            BackCommand = new Command(async () =>
            {
                if (UserType == "Patient")
                {
                    await Shell.Current.GoToAsync("///PatientHomePage");
                }
                else
                {
                    await Shell.Current.GoToAsync("///DoctorDashboardPage");
                }
            });

            // Initial load
            _ = LoadAppointments();
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
                    if (!appt.appointedAt.HasValue) continue;

                    string status = appt.status ?? "Pending";
                    var (cardBg, badgeBg) = GetColors(status);

                    string mainHeaderName;   // The bold text
                    string subDetailText;    // The smaller text

                    if (isStaffView)
                    {
                        // STAFF SEE: "Patient: Ali" (Dr. Steven)
                        var patientDetails = await _nurseController.ViewPatientDetailsAsync(appt.patient_IC);
                        string pName = appt.Patient?.patient_name
                                       ?? patientDetails?.patient_name
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

                    uiItems.Add(new AppointmentHistoryItem
                    {
                        Time = appt.appointedAt.Value.ToString("hh:mm tt"),
                        Date = appt.appointedAt.Value.ToString("dd MMM yyyy"),
                        Details = subDetailText,
                        DoctorName = mainHeaderName,
                        Status = status,
                        BadgeColor = badgeBg,
                        StatusColor = Colors.White,
                        CardBackgroundColor = cardBg,
                        AppointmentId = appt.appointment_ID,
                        RawDate = appt.appointedAt.Value,

                        // Cancellation support: Restored 24h lead time constraint
                        IsCancellable = !isStaffView && status == "Pending" && (appt.appointedAt.Value - DateTime.Now).TotalHours >= 24,
                        CancelCommand = new Command(async () => await OnCancelAppointment(appt.appointment_ID, status, appt.appointedAt)),
                        ViewDetailsCommand = new Command(async () => await OnViewDetails(appt.appointment_ID))
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

            // 2. SORTING: Newest Date First
            var sorted = filtered
                .OrderBy(a => a.Status?.ToLower() switch
                {
                    "pending" => 0,
                    "completed" => 1,
                    "cancelled" => 2,
                    _ => 3
                })
                .ThenBy(a => Math.Abs((a.RawDate - DateTime.Now).Ticks))
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
                "completed" => (Color.FromArgb("#FFFDE7"), Color.FromArgb("#FFCA28")), // Yellow (Previous design)
                _ => (Colors.White, Colors.Gray)
            };
        }


        private async Task OnViewDetails(string appointmentId)
        {
            if (!string.IsNullOrEmpty(appointmentId))
            {
                if (UserType == "Doctor" || UserType == "Nurse")
                {
                    // Doctor/Nurse sees their specific details page
                    await Shell.Current.GoToAsync($"ConsultationDetailsPage?appointmentId={appointmentId}&isHistory=true");
                }
                else
                {
                    // Patient sees patient details page
                    await Shell.Current.GoToAsync($"PatientConsultationDetails?appointmentId={appointmentId}");
                }
            }
        }

        private async Task OnCancelAppointment(string appointmentId, string status, DateTime? appointedAt)
        {
            if (string.IsNullOrEmpty(appointmentId) || status != "Pending" || !appointedAt.HasValue)
                return;

            var timeRemaining = appointedAt.Value - DateTime.Now;

            if (timeRemaining.TotalHours < 24)
            {
                await Shell.Current.DisplayAlert("Cannot Cancel",
                    "Appointments can only be cancelled at least 24 hours in advance.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Confirm",
                "Are you sure you want to cancel this appointment?", "Yes", "No");

            if (confirm)
            {
                await Shell.Current.GoToAsync($"AppointmentCancellation?appointmentId={appointmentId}");
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
        public bool IsCancellable { get; set; } // New
        public ICommand CancelCommand { get; set; } // New
        public ICommand ViewDetailsCommand { get; set; } // New
    }
}