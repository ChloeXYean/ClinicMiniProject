using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Services;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(UserType), "UserType")]

    public sealed class AppointmentScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IAppointmentScheduleService _scheduleService;
        private readonly IStaffService _staffService;

        private DateTime _selectedDate = DateTime.Today;
        private AppointmentScheduleGridDto _grid = new() { Date = DateTime.Today };
        private bool _isNurseMode = false;
        private Staff _selectedDoctor;
        private ObservableCollection<Staff> _doctors = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsNurseMode
        {
            get => _isNurseMode;
            set => SetProperty(ref _isNurseMode, value);
        }

        public Staff SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                if (SetProperty(ref _selectedDoctor, value))
                {
                    LoadSchedule();
                }
            }
        }

        public ObservableCollection<Staff> Doctors
        {
            get => _doctors;
            set => SetProperty(ref _doctors, value);
        }

        public string UserType
        {
            set
            {
                IsNurseMode = value == "Nurse";

                if (IsNurseMode)
                {
                    LoadDoctors();
                }

                (HomeCommand as Command)?.ChangeCanExecute();
                HomeCommand = IsNurseMode
                    ? new Command(async () => await Shell.Current.GoToAsync("///NurseHomePage"))
                    : new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
                OnPropertyChanged(nameof(HomeCommand));

                // 4. Load the schedule
                LoadSchedule();

                StartAutoRefresh();
            }

        }

        private void StartAutoRefresh()
        {
            Application.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadSchedule();
                });
                return true;
            });
        }

        public ICommand HomeCommand { get; set; }
        public ICommand ChatCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand ViewPatientDetailsCommand { get; }

        public AppointmentScheduleViewModel(IAuthService authService, IAppointmentScheduleService scheduleService, IStaffService staffService)
        {
            _authService = authService;
            _scheduleService = scheduleService;
            _staffService = staffService;

            // Default commands (will be updated by UserType setter)
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));

            ChatCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            ViewPatientDetailsCommand = new Command<string>(OnViewPatientDetails);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    LoadSchedule();
                }
            }
        }

        public AppointmentScheduleGridDto Grid
        {
            get => _grid;
            private set
            {
                if (SetProperty(ref _grid, value))
                {
                    OnPropertyChanged(nameof(ServiceTypes));
                    OnPropertyChanged(nameof(Rows));
                }
            }
        }

        public IReadOnlyList<string> ServiceTypes => Grid.ServiceTypes;
        public IReadOnlyList<TimeSlotRowDto> Rows => Grid.Rows;
        private async void LoadSchedule()
        {
            System.Diagnostics.Debug.WriteLine("=== LoadSchedule CALLED ===");
            System.Diagnostics.Debug.WriteLine($"IsNurseMode: {IsNurseMode}");
            System.Diagnostics.Debug.WriteLine($"SelectedDate: {SelectedDate:yyyy-MM-dd}");
            
            if (IsNurseMode)
            {
                if (SelectedDoctor == null)
                {
                    System.Diagnostics.Debug.WriteLine("No doctor selected in Nurse mode");
                    Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"Loading schedule for doctor: {SelectedDoctor.staff_ID}");
                try
                {
                    Grid = await _scheduleService.GetScheduleGridAsync(SelectedDoctor.staff_ID, SelectedDate.Date);
                    System.Diagnostics.Debug.WriteLine($"Loaded {Grid.Rows.Count} time slots");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading schedule: {ex.Message}");
                    Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                }
            }
            else
            {
                var current = _authService.GetCurrentUser();
                System.Diagnostics.Debug.WriteLine($"Current user: {current?.staff_ID} ({current?.staff_name})");
                
                if (current == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: No current user found!");
                    return;
                }

                try
                {
                    System.Diagnostics.Debug.WriteLine($"Loading schedule for doctor: '{current.staff_ID}' (Type: {current.staff_ID?.GetType().Name})");
                    Grid = await _scheduleService.GetScheduleGridAsync(current.staff_ID, SelectedDate.Date);
                    System.Diagnostics.Debug.WriteLine($"Loaded {Grid.Rows.Count} time slots");
                    
                    // Debug: Count booked slots
                    int bookedCount = 0;
                    foreach (var row in Grid.Rows)
                    {
                        foreach (var kvp in row.CellsByServiceType)
                        {
                            if (kvp.Value.IsBooked && !string.IsNullOrEmpty(kvp.Value.PatientName))
                            {
                                bookedCount++;
                                System.Diagnostics.Debug.WriteLine($"  [{row.SlotStart:HH:mm}] {kvp.Key}: {kvp.Value.PatientName} (IC: {kvp.Value.PatientIc})");
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Total booked slots displayed: {bookedCount}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR loading schedule: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                }
            }
            
            System.Diagnostics.Debug.WriteLine("=== LoadSchedule COMPLETED ===");
        }

        private void LoadDoctors()
        {
            try
            {
                var doctorList = _staffService.GetAllDocs();
                Doctors.Clear();
                foreach (var doc in doctorList)
                {
                    Doctors.Add(doc);
                }
                if (Doctors.Count > 0)
                {
                    SelectedDoctor = Doctors[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading doctors: {ex.Message}");
            }
        }

        private void OnViewPatientDetails(string patientIc)
        {
            if (string.IsNullOrWhiteSpace(patientIc))
                return;

            Shell.Current.GoToAsync($"PatientDetails?patientIc={Uri.EscapeDataString(patientIc)}&UserType=Doctor");
        }

        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
