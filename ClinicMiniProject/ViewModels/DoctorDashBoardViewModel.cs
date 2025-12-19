using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            // Initialize Data
            LoadDashboardData();
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
    }
}