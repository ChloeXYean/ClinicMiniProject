using System.Collections.ObjectModel;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Controller;

namespace ClinicMiniProject.ViewModels
{
    // DTO for walk-in patients (matching nurse controller structure)
    public class WalkInQueueDto
    {
        public string QueueId { get; set; }
        public string PatientName { get; set; }
        public string ICNumber { get; set; }
        public string ServiceType { get; set; }
        public string RegisteredTime { get; set; }
    }

    [QueryProperty(nameof(AppointmentId), "appointmentId")]
    public class ConsultationDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorDashboardService _dashboardService;
        private readonly NurseController _nurseController;

        // Single appointment properties (for backward compatibility)
        private string _appointmentId;
        private string _appointmentDate;
        private string _appointmentTime;
        private string _patientName;
        private string _patientIC;
        private string _serviceType;
        private string _doctorRemarks;
        private string _nurseNotes;
        private string _prescription;
        private string _status;

        // New list-based properties
        private ObservableCollection<ConsultationAppointmentDto> _appointmentsList = new();
        private ObservableCollection<ConsultationAppointmentDto> _allAppointments = new();
        private ConsultationAppointmentDto _selectedAppointment;
        private string _consultationRemarks = string.Empty;
        private CurrentConsultationDto _currentConsultation = null;
        private string _completedConsultationRemarks = string.Empty;
        private bool _isConsultationCompleted = false;

        // Filter properties
        private string _filterPatientName = string.Empty;
        private string _filterPatientIC = string.Empty;
        private string _selectedBookingType = "All";
        private string _selectedTimeRange = "All Day";

        public string AppointmentId
        {
            get => _appointmentId;
            set
            {
                _appointmentId = value;
                OnPropertyChanged();
                _ = LoadConsultationDetails();
            }
        }

        public string AppointmentDate
        {
            get => _appointmentDate;
            set => SetProperty(ref _appointmentDate, value);
        }

        public string AppointmentTime
        {
            get => _appointmentTime;
            set => SetProperty(ref _appointmentTime, value);
        }

        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        public string PatientIC
        {
            get => _patientIC;
            set => SetProperty(ref _patientIC, value);
        }

        public string ServiceType
        {
            get => _serviceType;
            set => SetProperty(ref _serviceType, value);
        }

        public string DoctorRemarks
        {
            get => _doctorRemarks;
            set => SetProperty(ref _doctorRemarks, value);
        }

        public string NurseNotes
        {
            get => _nurseNotes;
            set => SetProperty(ref _nurseNotes, value);
        }

        public string Prescription
        {
            get => _prescription;
            set => SetProperty(ref _prescription, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // New properties for appointment list
        public ObservableCollection<ConsultationAppointmentDto> AppointmentsList
        {
            get => _appointmentsList;
            set => SetProperty(ref _appointmentsList, value);
        }

        public ConsultationAppointmentDto SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                SetProperty(ref _selectedAppointment, value);
                // Update single appointment properties for backward compatibility
                if (value != null)
                {
                    AppointmentId = value.AppointmentId;
                    PatientName = value.PatientName;
                    PatientIC = value.PatientIC;
                    ServiceType = value.ServiceType;
                    AppointmentTime = value.AppointmentTime.ToString("hh:mm tt");
                    AppointmentDate = value.AppointmentTime.ToString("dd MMM yyyy");
                    Status = value.Status;
                }
            }
        }

        public string ConsultationRemarks
        {
            get => _consultationRemarks;
            set => SetProperty(ref _consultationRemarks, value);
        }

        public CurrentConsultationDto CurrentConsultation
        {
            get => _currentConsultation;
            set => SetProperty(ref _currentConsultation, value);
        }

        public string CompletedConsultationRemarks
        {
            get => _completedConsultationRemarks;
            set => SetProperty(ref _completedConsultationRemarks, value);
        }

        // Computed properties
        public bool HasNoAppointments => AppointmentsList?.Count == 0;
        public bool HasAppointments => AppointmentsList?.Count > 0;
        public bool HasSelectedAppointment => SelectedAppointment != null;
        public bool CanStartSelectedConsultation => HasSelectedAppointment && SelectedAppointment.CanStartConsultation && CurrentConsultation == null;
        public bool IsConsultationInProgress => CurrentConsultation != null;
        public bool IsConsultationCompleted => _isConsultationCompleted;

        public ICommand BackCommand { get; }
        public ICommand StartConsultationCommand { get; }
        public ICommand StartSelectedConsultationCommand { get; }
        public ICommand EndConsultationCommand { get; }
        public ICommand SaveRemarksCommand { get; }
        public ICommand EditRemarksCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public string FilterPatientName
        {
            get => _filterPatientName;
            set => SetProperty(ref _filterPatientName, value);
        }

        public string FilterPatientIC
        {
            get => _filterPatientIC;
            set => SetProperty(ref _filterPatientIC, value);
        }

        public string SelectedBookingType
        {
            get => _selectedBookingType;
            set => SetProperty(ref _selectedBookingType, value);
        }

        public string SelectedTimeRange
        {
            get => _selectedTimeRange;
            set => SetProperty(ref _selectedTimeRange, value);
        }

        public List<string> BookingTypes { get; } = new() { "All", "Online", "Walk-in" };
        public List<string> TimeRanges { get; } = new() { "All Day", "Morning (9AM-12PM)", "Afternoon (12PM-5PM)", "Evening (5PM-9PM)" };

        public bool HasActiveFilters => !string.IsNullOrEmpty(FilterPatientName) || 
                                       !string.IsNullOrEmpty(FilterPatientIC) || 
                                       SelectedBookingType != "All" || 
                                       SelectedTimeRange != "All Day";

        public ConsultationDetailsViewModel(IAuthService authService, IConsultationService consultationService, IAppointmentService appointmentService, IDoctorDashboardService dashboardService, NurseController nurseController)
        {
            _authService = authService;
            _consultationService = consultationService;
            _appointmentService = appointmentService;
            _dashboardService = dashboardService;
            _nurseController = nurseController;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));

            // New commands
            StartConsultationCommand = new Command<string>(async (appointmentId) => await StartConsultation(appointmentId));
            StartSelectedConsultationCommand = new Command(async () => await StartSelectedConsultation());
            EndConsultationCommand = new Command(async () => await EndConsultation());
            SaveRemarksCommand = new Command(async () => await SaveRemarks());
            EditRemarksCommand = new Command(async () => await EditRemarks());
            ViewDetailsCommand = new Command<ConsultationAppointmentDto>(async (appointment) => await ViewDetails(appointment));
            
            // Filter commands
            ApplyFilterCommand = new Command(async () => await ApplyFilter());
            ClearFilterCommand = new Command(async () => await ClearFilter());
        }

        private async Task LoadConsultationDetails()
        {
            if (string.IsNullOrEmpty(AppointmentId))
            {
                // If no specific appointment, load all appointments for today
                await LoadTodayAppointments();
                return;
            }

            // Load specific appointment (backward compatibility)
            await LoadSingleAppointment();
        }

        private async Task<List<WalkInQueueDto>> GetWalkInQueueForToday()
        {
            try
            {
                // Use the nurse controller to get walk-in patients
                var walkInQueue = await _nurseController.GetWalkInQueueForToday();
                
                return walkInQueue.Select(w => new WalkInQueueDto
                {
                    QueueId = w.QueueId,
                    PatientName = w.PatientName,
                    ICNumber = w.ICNumber,
                    ServiceType = w.ServiceType,
                    RegisteredTime = w.RegisteredTime
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting walk-in queue: {ex.Message}");
                return new List<WalkInQueueDto>();
            }
        }

        private async Task LoadTodayAppointments()
        {
            try
            {
                var currentUser = _authService.GetCurrentUser();
                if (currentUser == null) 
                {
                    System.Diagnostics.Debug.WriteLine("No current user found");
                    return;
                }

                var today = DateTime.Today;
                var now = DateTime.Now;
                
                System.Diagnostics.Debug.WriteLine($"=== LOADING APPOINTMENTS FOR DOCTOR: {currentUser.staff_ID} ===");
                System.Diagnostics.Debug.WriteLine($"Today's date: {today}");

                // Get today's scheduled appointments for this doctor
                var scheduledAppointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(
                    currentUser.staff_ID, today, today.AddDays(1));

                System.Diagnostics.Debug.WriteLine($"Found {scheduledAppointments?.Count() ?? 0} scheduled appointments");
                if (scheduledAppointments != null)
                {
                    foreach (var apt in scheduledAppointments)
                    {
                        System.Diagnostics.Debug.WriteLine($"Scheduled: {apt.appointment_ID} - {apt.Patient?.patient_name} - {apt.status}");
                    }
                }

                // Get walk-in patients - get ALL appointments for today and filter for unassigned/walk-in patients
                var walkInPatients = new List<ConsultationAppointmentDto>();
                
                // Get all appointments for today (not just this doctor's)
                var allTodayAppointments = await _appointmentService.GetAppointmentsByDateAsync(today);
                System.Diagnostics.Debug.WriteLine($"Total appointments for today: {allTodayAppointments?.Count() ?? 0}");
                
                if (allTodayAppointments != null)
                {
                    // Filter for walk-in patients (no assigned doctor OR assigned to this doctor with walk-in status)
                    var walkIns = allTodayAppointments
                        .Where(a => a.staff_ID == null || a.staff_ID == currentUser.staff_ID)
                        .Where(a => a.status == "Pending" || a.status == "Walk-in" || a.status == "Registered")
                        .Where(a => a.appointedAt.HasValue && a.appointedAt.Value.Date == today)
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"Found {walkIns.Count} potential walk-in patients");
                    
                    foreach (var walkIn in walkIns)
                    {
                        System.Diagnostics.Debug.WriteLine($"Walk-in: {walkIn.appointment_ID} - {walkIn.Patient?.patient_name} - staff_ID: {walkIn.staff_ID} - status: {walkIn.status}");
                    }
                    
                    walkInPatients = walkIns.Select(w => new ConsultationAppointmentDto
                    {
                        AppointmentId = w.appointment_ID,
                        PatientName = w.Patient?.patient_name ?? "Unknown Patient",
                        PatientIC = w.patient_IC ?? "Unknown IC",
                        ServiceType = w.service_type?.ToString() ?? "General Consultation",
                        AppointmentTime = w.appointedAt ?? DateTime.Now,
                        Status = w.staff_ID == null ? "Walk-in" : (w.status ?? "Pending"),
                        CanStartConsultation = true,
                        CanStartEarly = true,
                        TimeFromNow = TimeSpan.Zero,
                        TimeIndicatorColor = "#28A745", // Green for walk-ins
                        StatusColor = GetStatusColor(w.staff_ID == null ? "Walk-in" : (w.status ?? "Pending"))
                    }).ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"Converted {walkInPatients.Count} walk-in patients");
                }

                // Combine both scheduled and walk-in appointments
                var allAppointments = scheduledAppointments
                    .Where(a => a.appointedAt.HasValue)
                    .Select(a => new ConsultationAppointmentDto
                    {
                        AppointmentId = a.appointment_ID,
                        PatientName = a.Patient?.patient_name ?? "Unknown Patient",
                        PatientIC = a.patient_IC ?? "Unknown IC",
                        ServiceType = a.service_type?.ToString() ?? "General Consultation",
                        AppointmentTime = a.appointedAt.Value,
                        Status = a.status ?? "Pending",
                        CanStartConsultation = a.status == "Pending",
                        CanStartEarly = a.appointedAt.Value > now,
                        TimeFromNow = a.appointedAt.Value - now,
                        TimeIndicatorColor = GetTimeIndicatorColor(a.appointedAt.Value, now),
                        StatusColor = GetStatusColor(a.status ?? "Pending")
                    })
                    .Concat(walkInPatients)
                    .OrderBy(a => a.AppointmentTime)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Combined total appointments: {allAppointments.Count}");

                // Clear and update both collections
                AppointmentsList.Clear();
                _allAppointments.Clear();
                
                foreach (var appointment in allAppointments)
                {
                    AppointmentsList.Add(appointment);
                    _allAppointments.Add(appointment);
                    System.Diagnostics.Debug.WriteLine($"Added: {appointment.PatientName} - {appointment.ServiceType}");
                }

                // Auto-select the first appointment (closest to current time)
                if (AppointmentsList.Count > 0)
                {
                    SelectedAppointment = AppointmentsList[0];
                    System.Diagnostics.Debug.WriteLine($"Auto-selected: {SelectedAppointment.PatientName}");
                }
                
                System.Diagnostics.Debug.WriteLine($"=== FINAL: Total appointments shown: {AppointmentsList.Count} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading today's appointments: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load appointments", "OK");
            }
        }

        private async Task LoadSingleAppointment()
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
                if (appointment != null)
                {
                    AppointmentDate = appointment.appointedAt?.ToString("dd MMM yyyy") ?? " - ";
                    AppointmentTime = appointment.appointedAt?.ToString("hh:mm tt") ?? " - ";
                    PatientName = appointment.Patient?.patient_name ?? " - ";
                    PatientIC = appointment.patient_IC ?? " - ";
                    ServiceType = appointment.service_type?.ToString() ?? "General Consultation";
                    Status = appointment.status ?? " - ";

                    // Check if consultation is completed
                    _isConsultationCompleted = appointment.status == "Completed";

                    // Load consultation details if available
                    var consultation = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(AppointmentId);
                    if (consultation != null)
                    {
                        DoctorRemarks = !string.IsNullOrWhiteSpace(consultation.DoctorRemark) ? consultation.DoctorRemark : "No doctor remarks available ";
                        NurseNotes = !string.IsNullOrWhiteSpace(consultation.NurseRemark) ? consultation.NurseRemark : "No Nurse remarks available";
                        CompletedConsultationRemarks = consultation.DoctorRemark ?? "No remarks available";
                        Prescription = " - "; // Or fetch prescription if you have that logic later
                    }
                    else
                    {
                        DoctorRemarks = " - ";
                        NurseNotes = " - ";
                        Prescription = " - ";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading consultation details: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load consultation details.", "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private string GetTimeIndicatorColor(DateTime appointmentTime, DateTime now)
        {
            var timeDiff = appointmentTime - now;
            
            if (timeDiff.TotalMinutes <= 30)
                return "#28A745"; // Green - very soon or current
            else if (timeDiff.TotalHours <= 2)
                return "#FFA500"; // Orange - soon
            else if (timeDiff.TotalHours <= 6)
                return "#2D5AF0"; // Blue - today
            else
                return "#6C757D"; // Gray - later
        }

        private string GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "#FFA500",
                "in progress" => "#28A745",
                "completed" => "#6C757D",
                "cancelled" => "#DC3545",
                "emergency" => "#DC3545",
                _ => "#6C757D"
            };
        }

        private async Task StartConsultation(string appointmentId)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Appointment not found", "OK");
                    return;
                }

                // Update appointment time to current time (early start)
                var originalTime = appointment.appointedAt;
                appointment.appointedAt = DateTime.Now;
                appointment.status = "In Progress";
                
                await _appointmentService.UpdateAppointmentAsync(appointment);

                // Set current consultation
                CurrentConsultation = new CurrentConsultationDto
                {
                    AppointmentId = appointmentId,
                    PatientName = appointment.Patient?.patient_name ?? "Unknown Patient",
                    PatientIC = appointment.patient_IC ?? "Unknown IC",
                    ServiceType = appointment.service_type?.ToString() ?? "General Consultation",
                    StartTime = DateTime.Now,
                    OriginalAppointmentTime = originalTime ?? DateTime.Now
                };

                // Clear remarks for new consultation
                ConsultationRemarks = string.Empty;

                // Refresh the appointments list
                await LoadTodayAppointments();

                await Shell.Current.DisplayAlert("Success", "Consultation started successfully", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting consultation: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to start consultation", "OK");
            }
        }

        private async Task StartSelectedConsultation()
        {
            if (SelectedAppointment == null)
            {
                await Shell.Current.DisplayAlert("Error", "No appointment selected", "OK");
                return;
            }

            await StartConsultation(SelectedAppointment.AppointmentId);
        }

        private async Task EndConsultation()
        {
            try
            {
                if (CurrentConsultation == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No active consultation", "OK");
                    return;
                }

                // Update appointment status to completed
                var appointment = await _appointmentService.GetAppointmentByIdAsync(CurrentConsultation.AppointmentId);
                if (appointment != null)
                {
                    appointment.status = "Completed";
                    await _appointmentService.UpdateAppointmentAsync(appointment);

                    // Save consultation remarks if available
                    if (!string.IsNullOrWhiteSpace(ConsultationRemarks))
                    {
                        await SaveConsultationDetails(CurrentConsultation.AppointmentId, ConsultationRemarks);
                    }
                }

                // Clear current consultation
                CurrentConsultation = null;
                ConsultationRemarks = string.Empty;

                // Refresh the appointments list
                await LoadTodayAppointments();

                await Shell.Current.DisplayAlert("Success", "Consultation ended successfully", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ending consultation: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to end consultation", "OK");
            }
        }

        private async Task SaveRemarks()
        {
            try
            {
                if (CurrentConsultation == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No active consultation", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ConsultationRemarks))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter consultation remarks", "OK");
                    return;
                }

                await SaveConsultationDetails(CurrentConsultation.AppointmentId, ConsultationRemarks);
                await Shell.Current.DisplayAlert("Success", "Remarks saved successfully", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving remarks: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to save remarks", "OK");
            }
        }

        private async Task SaveConsultationDetails(string appointmentId, string remarks)
        {
            // This would save to the consultation table
            // For now, we'll just log it - you'll need to implement the actual consultation service method
            System.Diagnostics.Debug.WriteLine($"Saving consultation remarks for appointment {appointmentId}: {remarks}");
            
            // TODO: Implement actual consultation details saving
            // await _consultationService.SaveConsultationDetailsAsync(appointmentId, remarks);
        }

        private async Task ViewDetails(ConsultationAppointmentDto appointment)
        {
            try
            {
                if (appointment == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No appointment selected", "OK");
                    return;
                }

                // Navigate to details page with appointment ID
                await Shell.Current.GoToAsync($"ConsultationDetailsPage?appointmentId={appointment.AppointmentId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error viewing details: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to view appointment details", "OK");
            }
        }

        private async Task EditRemarks()
        {
            try
            {
                if (string.IsNullOrEmpty(AppointmentId))
                {
                    await Shell.Current.DisplayAlert("Error", "No appointment selected", "OK");
                    return;
                }

                // Navigate to edit remarks page or show a dialog
                await Shell.Current.GoToAsync($"EditConsultationRemarksPage?appointmentId={AppointmentId}&currentRemarks={CompletedConsultationRemarks}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing remarks: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to edit remarks", "OK");
            }
        }

        // Filter methods
        private async Task ApplyFilter()
        {
            try
            {
                if (_allAppointments.Count == 0)
                {
                    // Store all appointments if not already stored
                    foreach (var appointment in AppointmentsList)
                    {
                        _allAppointments.Add(appointment);
                    }
                }

                var filteredAppointments = _allAppointments.AsEnumerable();

                // Filter by patient name
                if (!string.IsNullOrEmpty(FilterPatientName))
                {
                    filteredAppointments = filteredAppointments.Where(a => 
                        a.PatientName.Contains(FilterPatientName, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by IC number
                if (!string.IsNullOrEmpty(FilterPatientIC))
                {
                    filteredAppointments = filteredAppointments.Where(a => 
                        a.PatientIC.Contains(FilterPatientIC, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by booking type
                if (SelectedBookingType != "All")
                {
                    filteredAppointments = filteredAppointments.Where(a => 
                        GetBookingType(a) == SelectedBookingType);
                }

                // Filter by time range
                if (SelectedTimeRange != "All Day")
                {
                    filteredAppointments = filteredAppointments.Where(a => 
                        IsInTimeRange(a.AppointmentTime, SelectedTimeRange));
                }

                // Update the displayed list
                AppointmentsList.Clear();
                foreach (var appointment in filteredAppointments.OrderBy(a => a.AppointmentTime))
                {
                    AppointmentsList.Add(appointment);
                }

                // Auto-select first appointment if available
                if (AppointmentsList.Count > 0 && SelectedAppointment == null)
                {
                    SelectedAppointment = AppointmentsList[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying filter: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to apply filter", "OK");
            }
        }

        private async Task ClearFilter()
        {
            try
            {
                FilterPatientName = string.Empty;
                FilterPatientIC = string.Empty;
                SelectedBookingType = "All";
                SelectedTimeRange = "All Day";

                // Restore all appointments
                AppointmentsList.Clear();
                foreach (var appointment in _allAppointments.OrderBy(a => a.AppointmentTime))
                {
                    AppointmentsList.Add(appointment);
                }

                // Auto-select first appointment
                if (AppointmentsList.Count > 0)
                {
                    SelectedAppointment = AppointmentsList[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing filter: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to clear filter", "OK");
            }
        }

        private string GetBookingType(ConsultationAppointmentDto appointment)
        {
            // This is a simplified logic - you may need to adjust based on your actual data model
            // For now, we'll assume patients with isAppUser = false are walk-ins
            // You might need to add this information to the ConsultationAppointmentDto
            return "Online"; // Placeholder - implement based on your actual data
        }

        private bool IsInTimeRange(DateTime appointmentTime, string timeRange)
        {
            var hour = appointmentTime.Hour;
            
            return timeRange switch
            {
                "Morning (9AM-12PM)" => hour >= 9 && hour < 12,
                "Afternoon (12PM-5PM)" => hour >= 12 && hour < 17,
                "Evening (5PM-9PM)" => hour >= 17 && hour < 21,
                _ => true
            };
        }
    }
}