using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public class DoctorDashboardViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IDoctorDashboardService _dashboardService;
        private string _greeting;
        private TodayStatsDto _todayStats;
        private UpcomingScheduleDto _upcomingSchedule;
        private string _nextAppointmentTime;

        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);
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

        public event PropertyChangedEventHandler PropertyChanged;

        public DoctorDashboardViewModel(IAuthService authService, IDoctorDashboardService dashboardService)
        {
            _authService = authService;
            _dashboardService = dashboardService;

            // Initialize commands
            LogoutCommand = new Command(OnLogout);
            NavigateToAppointmentScheduleCommand = new Command(OnAppointmentSchedule);
            NavigateToConsultationDetailsCommand = new Command(OnConsultationDetails);
            NavigateToAppointmentHistoryCommand = new Command(OnAppointmentHistory);
            NavigateToReportingManagementCommand = new Command(OnReportingManagement);
            NavigateToHomeCommand = new Command(OnHome);
            NavigateToInquiryCommand = new Command(OnInquiry);
            NavigateToProfileCommand = new Command(OnProfile);

            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null) return;

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

        private void OnLogout()
        {
            _authService.Logout();
            // Navigate to login page
            Shell.Current.GoToAsync("//Login");
        }

        private void OnAppointmentSchedule()
        {
            // Navigate to appointment schedule page
            Shell.Current.GoToAsync("//AppointmentSchedule");
        }

        private void OnConsultationDetails()
        {
            // Navigate to consultation details page
            Shell.Current.GoToAsync("//ConsultationDetails");
        }

        private void OnAppointmentHistory()
        {
            // Navigate to appointment history page
            Shell.Current.GoToAsync("//AppointmentHistory");
        }

        private void OnReportingManagement()
        {
            // Navigate to reporting management page
            Shell.Current.GoToAsync("//ReportingManagement");
        }

        private void OnHome()
        {
            // Navigate to home (refresh)
            LoadDashboardData();
        }

        private void OnInquiry()
        {
            // Navigate to inquiry page
            Shell.Current.GoToAsync("//Inquiry");
        }

        private void OnProfile()
        {
            // Navigate to profile page
            Shell.Current.GoToAsync("//Profile");
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