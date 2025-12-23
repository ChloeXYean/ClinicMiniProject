using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Controller;
using ClinicMiniProject.UI.Nurse;
using ClinicMiniProject.UI.Doctor;

namespace ClinicMiniProject.ViewModels
{
    public class NurseHomeViewModel : INotifyPropertyChanged
    {
        private readonly NurseController _controller;

        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        private string date = string.Empty;
        public string AppDate
        {
            get => date;
            set => SetProperty(ref date, value);
        }

        private string time = string.Empty;
        public string AppTime
        {
            get => time;
            set => SetProperty(ref time, value);
        }

        private string doctor = string.Empty;
        public string AppDoc
        {
            get => doctor;
            set => SetProperty(ref doctor, value);
        }

        private string pendingCount = "0";
        public string PendingCount
        {
            get => pendingCount;
            set => SetProperty(ref pendingCount, value);
        }

        // Commands
        public ICommand HomeCommand { get; }
        public ICommand InquiryCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand RegisterPatientCommand { get; }
        public ICommand EndConsultationCommand { get; }
        public ICommand ViewAppointmentCommand { get; }
        public ICommand AppointmentHistoryCommand { get; }
        public ICommand ReportingManagementCommand { get; }
        public ICommand WalkInQueueCommand { get; }

        // Constructor
        public NurseHomeViewModel(NurseController controller)
        {
            _controller = controller;

            // --- 2. Initialize the Welcome Message HERE (inside constructor) ---
            string userName = Preferences.Get("CurrentUserName", "Nurse");
            WelcomeMessage = $"Hi, {userName}";

            // Navigation Commands
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}"));
            InquiryCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseProfile"));

            RegisterPatientCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(RegisterPatientPage)));
            EndConsultationCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(EndConsultationPage)));

            // Fixed String Interpolation for parameters
            ViewAppointmentCommand = new Command(async () => await Shell.Current.GoToAsync($"{nameof(AppointmentSchedulePage)}?UserType=Nurse"));
            AppointmentHistoryCommand = new Command(async () => await Shell.Current.GoToAsync($"NurseAppointmentHistory?UserType=Nurse"));
            ReportingManagementCommand = new Command(async () => await Shell.Current.GoToAsync($"NurseReporting?UserType=Nurse"));
            WalkInQueueCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(WalkInPatientQueuePage)));

            LoadDashboardData();
            StartAutoRefresh();
        }

        private void StartAutoRefresh()
        {
            Application.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                LoadDashboardData();
                return true;
            });
        }

        public async void LoadDashboardData()
        {
            AppDate = $"Date: {DateTime.Now:dd MMMM yyyy}";

            // Set defaults while loading
            if (string.IsNullOrEmpty(AppTime)) AppTime = "Time: --:--";
            if (string.IsNullOrEmpty(AppDoc)) AppDoc = "Doctor: --";

            try
            {
                var upcomingList = await _controller.GetUpcomingAppointment();

                // Safety check: ensure list is not null
                if (upcomingList != null)
                {
                    var nextAppointment = upcomingList.FirstOrDefault();

                    if (nextAppointment != null && nextAppointment.appointedAt.HasValue)
                    {
                        AppTime = $"Time: {nextAppointment.appointedAt.Value:hh:mm tt}";
                        AppDoc = $"Doctor: {nextAppointment.staff_ID}";
                    }

                    PendingCount = upcomingList.Count.ToString();
                }
                else
                {
                    PendingCount = "0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }
        }

        // --- Standard INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}