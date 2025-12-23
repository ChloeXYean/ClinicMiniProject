using ClinicMiniProject.Dtos;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(Patient), "SelectedPatient")]
    [QueryProperty(nameof(UserType), "UserType")]
    [QueryProperty(nameof(PatientId), "patientIc")]
    public class PatientDetailsViewModel : BindableObject
    {
        private readonly IAuthService _authService;
        private readonly PatientService _patientService;
        private readonly IAppointmentService _appointmentService;

        public ObservableCollection<Appointment> Appointments { get; } = new();

        // --- DTO PROPERTY (From Queue) ---
        private PatientQueueDto patient = new();
        public PatientQueueDto Patient
        {
            get => patient;
            set
            {
                patient = value;
                OnPropertyChanged();

                // If we receive a patient from the queue, we are in Nurse Mode
                if (patient != null)
                {
                    IsNurseView = true;
                    LoadFromQueueDto(patient);
                }
            }
        }

        // --- NEW: EXPLICIT MODE SWITCHING ---
        public string UserType
        {
            set
            {
                if (value == "Patient")
                {
                    IsNurseView = false;
                    IsDoctorView = false;
                    LoadCurrentPatientProfile();
                }
                else if (value == "Nurse")
                {
                    IsNurseView = true;
                    IsDoctorView = false;
                }
                else if (value == "Doctor")
                {
                    IsNurseView = false;
                    IsDoctorView = true;
                    LoadCurrentPatientProfile();
                }
            }
        }

        // --- NEW: DIRECT ID LOADING ---
        public string PatientId
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    LoadPatientData(value);
                }
            }
        }

        // --- VIEW STATE ---
        private bool isNurseView;
        public bool IsNurseView
        {
            get => isNurseView;
            set
            {
                isNurseView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPatientView));
                OnPropertyChanged(nameof(IsDoctorView));
            }
        }
        public bool IsPatientView => !IsNurseView && !IsDoctorView;
        
        private bool isDoctorView;
        public bool IsDoctorView
        {
            get => isDoctorView;
            private set
            {
                isDoctorView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPatientView));
            }
        }

        // --- DISPLAY PROPERTIES (Kept exact names for XAML) ---
        private string patientName = "Loading...";
        public string PatientName { get => patientName; set { patientName = value; OnPropertyChanged(); } }

        private string queueNo = "--";
        public string QueueNo { get => queueNo; set { queueNo = value; OnPropertyChanged(); } }

        private string icNumber = "--";
        public string IcNumber { get => icNumber; set { icNumber = value; OnPropertyChanged(); } }

        private string gender = "Unknown";
        public string Gender { get => gender; set { gender = value; OnPropertyChanged(); } }

        private string contact = "--";
        public string Contact { get => contact; set { contact = value; OnPropertyChanged(); } }

        private string email = "--";
        public string Email { get => email; set { email = value; OnPropertyChanged(); } }

        private string regTime = "--";
        public string RegTime { get => regTime; set { regTime = value; OnPropertyChanged(); } }

        private string reason = "N/A";
        public string Reason { get => reason; set { reason = value; OnPropertyChanged(); } }

        private ImageSource profilePictureSource;
        public ImageSource ProfilePictureSource
        {
            get => profilePictureSource;
            set { profilePictureSource = value; OnPropertyChanged(); }
        }

        // --- COMMANDS ---
        public ICommand BackCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CancelAppointmentCommand { get; }

        // --- CONSTRUCTOR ---
        public PatientDetailsViewModel(IAuthService authService, PatientService patientService, IAppointmentService appointmentService)
        {
            _authService = authService;
            _patientService = patientService;
            _appointmentService = appointmentService;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            UpdateCommand = new Command(OnUpdate);
            EditProfileCommand = new Command(async () => await OnEditProfile());
            LogoutCommand = new Command(async () => await OnLogout());

            CancelAppointmentCommand = new Command<Appointment>(async (a) => await OnCancelAppointment(a));

            // Auto-detect source if no parameters were passed yet
            CheckInitialSource();
        }

        // --- LOGIC METHODS ---

        private void CheckInitialSource()
        {
            var currentUser = _authService.GetCurrentUser(); // Is it a Staff?

            // If NOT staff, try loading as Patient
            if (currentUser == null)
            {
                var patient = _authService.GetCurrentPatient();
                if (patient != null)
                {
                    IsNurseView = false;
                    LoadPatientData(patient.patient_IC);
                }
                else
                {
                    IsNurseView = true; // Default fall back
                }
            }
            else
            {
                IsNurseView = true;
            }
        }

        private void LoadFromQueueDto(PatientQueueDto dto)
        {
            QueueNo = dto.QueueId ?? "--";
            RegTime = dto.RegisteredTime ?? "--";

            Reason = !string.IsNullOrEmpty(dto.ServiceType) ? dto.ServiceType : "General Consultation";

            PatientName = dto.PatientName ?? "Unknown";
            IcNumber = dto.ICNumber ?? "--";
            Contact = dto.PhoneNumber ?? "--";

            if (!string.IsNullOrEmpty(dto.ICNumber))
            {
                LoadPatientData(dto.ICNumber, overwriteVisitDetails: false);
            }
        }

        private void LoadCurrentPatientProfile()
        {
            var patient = _authService.GetCurrentPatient();
            if (patient != null)
            {
                LoadPatientData(patient.patient_IC, overwriteVisitDetails: true);

                QueueNo = "";
                RegTime = "";
                Reason = "";
            }
        }

        private async void LoadPatientData(string ic, bool overwriteVisitDetails = true)
        {
            var p = _patientService.GetPatientByIC(ic);
            if (p != null)
            {
                PatientName = p.patient_name;
                IcNumber = p.patient_IC;
                Contact = p.patient_contact;
                Email = p.patient_email ?? "N/A";

                Gender = "Unknown";

                ProfilePictureSource = ImageSource.FromFile("profilepicture.png");
            }
            await LoadAppointments(ic);
        }

        public void ReloadData()
        {
            if (!IsNurseView)
            {
                LoadCurrentPatientProfile();
            }
        }

        // --- ACTIONS ---
        private async void OnUpdate()
        {
            if (!string.IsNullOrEmpty(IcNumber))
            {
                // Pass the current patient's IC to the edit page
                await Shell.Current.GoToAsync($"EditPatientProfile?TargetPatientIC={IcNumber}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Cannot edit: No Patient IC found.", "OK");
            }
        }
        private async Task OnEditProfile()
        {
            await Shell.Current.GoToAsync("EditPatientProfile");
        }

        private async Task OnLogout()
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                _authService.Logout();
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }

        // -- Nurse --
        private async Task LoadAppointments(string ic)
        {
            try
            {
                var appts = await _appointmentService.GetAppointmentsByPatientIcAsync(ic);
                Appointments.Clear();
                foreach (var appt in appts) Appointments.Add(appt);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}"); }
        }

        private async Task OnCancelAppointment(Appointment appt)
        {
            if (appt == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Confirm", "Cancel this appointment?", "Yes", "No");
            if (!confirm) return;

            bool success = await _appointmentService.CancelAppointmentAsync(appt.appointment_ID);
            if (success) { await LoadAppointments(appt.patient_IC); }
        }

    }
}