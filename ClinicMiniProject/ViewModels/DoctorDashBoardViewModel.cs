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
        private string _greeting = "Welcome";
        private TodayStatsDto _todayStats = new();
        private UpcomingScheduleDto _upcomingSchedule = new();
        private string _nextAppointmentTime = "Loading...";
        private string _doctorName = "Doctor";
        private string _searchText = string.Empty;
        private ObservableCollection<InquiryDto> _allInquiries = new();
        private ObservableCollection<InquiryDto> _filteredInquiries = new();

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

        // Commands for menu items
        public ICommand LogoutCommand { get; }
        public ICommand NavigateToAppointmentScheduleCommand { get; }
        public ICommand NavigateToConsultationDetailsCommand { get; }
        public ICommand NavigateToAppointmentHistoryCommand { get; }
        public ICommand NavigateToReportingManagementCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToInquiryCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToInquiryHistoryCommand { get; }

        public ICommand ToggleMenuCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand ViewInquiryDetailsCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DoctorDashboardViewModel(IAuthService authService, IDoctorDashboardService dashboardService)
        {
            _authService = authService;
            _dashboardService = dashboardService;

            // --- Navigation Logic ---
            NavigateToAppointmentScheduleCommand = new Command(async () => await Shell.Current.GoToAsync("///AppointmentSchedulePage"));
            NavigateToConsultationDetailsCommand = new Command(async () => await Shell.Current.GoToAsync("///ConsultationDetailsPage"));
            NavigateToAppointmentHistoryCommand = new Command(async () => await Shell.Current.GoToAsync("///AppointmentHistoryPage"));
            NavigateToReportingManagementCommand = new Command(async () => await Shell.Current.GoToAsync("///ReportingManagementPage"));

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
            NavigateToConsultationDetailsCommand = new Command(async () => await Shell.Current.GoToAsync("ConsultationDetails"));
            ToggleMenuCommand = new Command(() => { /* TODO: Implement Toggle Menu */ });
            NotificationCommand = new Command(async () => await Shell.Current.DisplayAlert("Notification", "No new notifications", "OK"));
            NavigateToHomeCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
            ViewInquiryDetailsCommand = new Command<string>(async (inquiryId) => await NavigateToInquiryDetails(inquiryId));

            // Initialize Data
            LoadDashboardData();
            LoadInquiryData();
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

        private void LoadInquiryData()
        {
            System.Diagnostics.Debug.WriteLine("=== Loading hardcoded inquiry data ===");
            
            // Load sample inquiry data for demonstration
            AllInquiries.Clear();
            AllInquiries.Add(new InquiryDto 
            { 
                InquiryId = "1", 
                PatientName = "John Doe", 
                CreatedAt = DateTime.Now.AddDays(-2),
                FullSymptomDescription = "I have been experiencing headaches for the past few days..."
            });
            AllInquiries.Add(new InquiryDto 
            { 
                InquiryId = "2", 
                PatientName = "Jane Smith", 
                CreatedAt = DateTime.Now.AddDays(-1),
                FullSymptomDescription = "I need a prescription refill for my blood pressure medication..."
            });
            AllInquiries.Add(new InquiryDto 
            { 
                InquiryId = "3", 
                PatientName = "Mike Johnson", 
                CreatedAt = DateTime.Now,
                FullSymptomDescription = "I have a fever and sore throat, should I come in for a checkup?"
            });

            // Initialize filtered inquiries with all inquiries
            FilteredInquiries.Clear();
            foreach (var inquiry in AllInquiries)
            {
                FilteredInquiries.Add(inquiry);
            }
            
            System.Diagnostics.Debug.WriteLine($"=== Loaded {AllInquiries.Count} hardcoded inquiries ===");
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
    }
}