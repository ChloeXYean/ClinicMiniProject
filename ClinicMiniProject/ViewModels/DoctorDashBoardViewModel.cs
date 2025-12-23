using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI.Doctor;

namespace ClinicMiniProject.ViewModels
{
    public class DoctorDashboardViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IDoctorDashboardService _dashboardService;
        private readonly IInquiryService _inquiryService;
        private readonly IAppointmentService _appointmentService;
        private readonly IConsultationService _consultationService;
        private string _greeting = "Welcome";
        private TodayStatsDto _todayStats = new();
        private UpcomingScheduleDto _upcomingSchedule = new();
        private string _nextAppointmentTime = "Loading...";
        private string _doctorName = "Doctor";
        private string _searchText = string.Empty;
        private ObservableCollection<InquiryDto> _allInquiries = new();
        private ObservableCollection<InquiryDto> _filteredInquiries = new();
        private string _consultationRemarks = string.Empty;
        private CurrentConsultationDto _currentConsultation = null;

        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);
        }

        public string DoctorName
        {
            get => _doctorName;
            set => SetProperty(ref _doctorName, value);
        }

        public string CurrentDate => DateTime.Now.ToString("dddd, MMMM dd, yyyy");

        public TodayStatsDto TodayStats
        {
            get => _todayStats;
            set => SetProperty(ref _todayStats, value);
        }

        public UpcomingScheduleDto UpcomingSchedule
        {
            get => _upcomingSchedule;
            set => SetProperty(ref _upcomingSchedule, value);
        }

        public string NextAppointmentTime
        {
            get => _nextAppointmentTime;
            set => SetProperty(ref _nextAppointmentTime, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ObservableCollection<InquiryDto> AllInquiries
        {
            get => _allInquiries;
            set => SetProperty(ref _allInquiries, value);
        }

        public ObservableCollection<InquiryDto> FilteredInquiries
        {
            get => _filteredInquiries;
            set
            {
                _filteredInquiries = value;
                OnPropertyChanged(nameof(FilteredInquiries));
                System.Diagnostics.Debug.WriteLine($"FilteredInquiries property updated - new count: {value?.Count ?? 0}");
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

        public bool HasNoAppointments => UpcomingSchedule?.PendingAppointments == 0;
        public bool HasNextAppointment => UpcomingSchedule?.NextAppointmentDetails != null && CurrentConsultation == null;
        public bool IsConsultationInProgress => CurrentConsultation != null;

        // Commands for menu items
        public ICommand LogoutCommand { get; }
        public ICommand NavigateToAppointmentScheduleCommand { get; }
        public ICommand NavigateToAppointmentHistoryCommand { get; }
        public ICommand NavigateToReportingManagementCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToInquiryCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToInquiryHistoryCommand { get; }
        public ICommand NavigateToConsultationDetailsCommand { get; }

        public ICommand ToggleMenuCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand ViewInquiryDetailsCommand { get; }
        public ICommand StartConsultationCommand { get; }
        public ICommand EndConsultationCommand { get; }
        public ICommand SaveRemarksCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DoctorDashboardViewModel(IAuthService authService, IDoctorDashboardService dashboardService, IInquiryService inquiryService, IAppointmentService appointmentService, IConsultationService consultationService)
        {
            _authService = authService;
            _dashboardService = dashboardService;
            _inquiryService = inquiryService;
            _appointmentService = appointmentService;
            _consultationService = consultationService;

            // --- Navigation Logic ---
            NavigateToAppointmentScheduleCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AppointmentSchedulePage)));
            NavigateToAppointmentHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AppointmentHistoryPage)));
            NavigateToReportingManagementCommand = new Command(async () => await Shell.Current.GoToAsync("///ReportingManagementPage"));
            NavigateToConsultationDetailsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(ConsultationDetailsPage)));

            // Bottom Bar Commands
            NavigateToInquiryCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
            NavigateToInquiryHistoryCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
            NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            NavigateToHomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));

            LogoutCommand = new Command(async () =>
            {
                _authService.Logout();
                await Shell.Current.GoToAsync($"///LoginPage");
            });

            // Initialize missing commands to avoid warnings
            ToggleMenuCommand = new Command(() => { /* TODO: Implement Toggle Menu */ });
            NotificationCommand = new Command(async () => await Shell.Current.DisplayAlert("Notification", "No new notifications", "OK"));
            NavigateToHomeCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
            ViewInquiryDetailsCommand = new Command<string>(async (inquiryId) => await NavigateToInquiryDetails(inquiryId));

            // Consultation commands
            StartConsultationCommand = new Command(async () => await StartConsultation());
            EndConsultationCommand = new Command(async () => await EndConsultation());
            SaveRemarksCommand = new Command(async () => await SaveRemarks());

            // Initialize Data
            LoadDashboardData();

            // Load inquiry data asynchronously without blocking constructor
            _ = Task.Run(async () => await LoadInquiryData());

            StartAutoRefresh();
        }

        private void StartAutoRefresh()
        {
            Application.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await LoadDashboardDataAsync();

                        if (string.IsNullOrEmpty(SearchText))
                        {
                            await LoadInquiryData();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in auto refresh: {ex.Message}");
                    }
                });

                return true;
            });
        }

        private async void LoadDashboardData()
        {
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null) return;

            DoctorName = currentUser.staff_name;
            Greeting = $"Hi, Dr. {currentUser.staff_name}";

            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync(currentUser.staff_ID);
                TodayStats = dashboardData.TodayStats;
                UpcomingSchedule = dashboardData.UpcomingSchedule;
                NextAppointmentTime = dashboardData.UpcomingSchedule.NextAppointmentTime?.ToString("h:mm tt") ?? "No upcoming appointments";
            }
            catch (Exception ex)
            {
                // Handle error (e.g., show error message)
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null) return;

            DoctorName = currentUser.staff_name;
            Greeting = $"Hi, Dr. {currentUser.staff_name}";

            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync(currentUser.staff_ID);
                TodayStats = dashboardData.TodayStats;
                UpcomingSchedule = dashboardData.UpcomingSchedule;
                NextAppointmentTime = dashboardData.UpcomingSchedule.NextAppointmentTime?.ToString("h:mm tt") ?? "No upcoming appointments";
            }
            catch (Exception ex)
            {
                // Handle error (e.g., show error message)
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task LoadInquiryData()
        {
            System.Diagnostics.Debug.WriteLine("=== Loading inquiry data from database ===");

            try
            {
                AllInquiries.Clear();
                FilteredInquiries.Clear();

                // Get current doctor
                var currentUser = _authService.GetCurrentUser();
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("No current user found");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Current user: {currentUser.staff_name}, ID: {currentUser.staff_ID}, IsDoctor: {currentUser.isDoctor}");

                // Fetch inquiries for current doctor only
                var inquiries = await _inquiryService.GetInquiriesByDoctorAsync(currentUser.staff_ID);

                foreach (var inquiry in inquiries)
                {
                    AllInquiries.Add(inquiry);
                    FilteredInquiries.Add(inquiry);
                    System.Diagnostics.Debug.WriteLine($"Added inquiry: {inquiry.InquiryId} - {inquiry.PatientName}");
                }

                System.Diagnostics.Debug.WriteLine($"=== Loaded {AllInquiries.Count} inquiries for Dr. {currentUser.staff_name} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading inquiry data: {ex.Message}");
                Console.WriteLine($"Error loading inquiry data: {ex.Message}");
            }
        }

        private async Task NavigateToInquiryDetails(string inquiryId)
        {
            if (!string.IsNullOrEmpty(inquiryId))
            {
                await Shell.Current.GoToAsync($"///InquiryDetailsPage?inquiryId={inquiryId}");
            }
        }

        public void FilterInquiries(string searchText)
        {
            System.Diagnostics.Debug.WriteLine($"=== FilterInquiries Called ===");
            System.Diagnostics.Debug.WriteLine($"Search text: '{searchText}'");
            System.Diagnostics.Debug.WriteLine($"AllInquiries count: {AllInquiries.Count}");

            // Clear current filtered inquiries
            FilteredInquiries.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Add all inquiries back
                foreach (var inquiry in AllInquiries)
                {
                    FilteredInquiries.Add(inquiry);
                }
                System.Diagnostics.Debug.WriteLine("Search text is empty - showing all inquiries");
            }
            else
            {
                // Add only matching inquiries
                var filtered = AllInquiries.Where(inquiry =>
                    inquiry.PatientName.ToLower().Contains(searchText.ToLower()) ||
                    inquiry.FullSymptomDescription.ToLower().Contains(searchText.ToLower()) ||
                    inquiry.CreatedAt.ToString("MMM dd, yyyy").ToLower().Contains(searchText.ToLower()));

                foreach (var inquiry in filtered)
                {
                    FilteredInquiries.Add(inquiry);
                }
                System.Diagnostics.Debug.WriteLine($"Filtered results count: {FilteredInquiries.Count}");
            }

            System.Diagnostics.Debug.WriteLine($"=== FilterInquiries Completed ===");
        }

        private async Task StartConsultation()
        {
            try
            {
                var nextAppointment = UpcomingSchedule?.NextAppointmentDetails;
                if (nextAppointment == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No upcoming appointment available", "OK");
                    return;
                }

                // Navigate to consultation details page with appointment ID
                await Shell.Current.GoToAsync($"///ConsultationDetailsPage?appointmentId={nextAppointment.AppointmentId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting consultation: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to start consultation", "OK");
            }
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

                // Refresh dashboard data
                LoadDashboardData();

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
    }
}