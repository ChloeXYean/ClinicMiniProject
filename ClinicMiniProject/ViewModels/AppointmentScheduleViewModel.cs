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
            System.Diagnostics.Debug.WriteLine($"=== LoadSchedule CALLED (Mode: {(IsNurseMode ? "Nurse" : "Doctor")}) ===");

            try
            {
                if (IsNurseMode)
                {
                    if (SelectedDoctor == null)
                    {
                        Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                        return;
                    }

                    if (SelectedDoctor.staff_ID == "ALL")
                    {
                        System.Diagnostics.Debug.WriteLine("Loading schedule for ALL doctors");

                        Grid = await _scheduleService.GetScheduleGridAsync(null, SelectedDate.Date);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Loading schedule for doctor: {SelectedDoctor.staff_name}");
                        Grid = await _scheduleService.GetScheduleGridAsync(SelectedDoctor.staff_ID, SelectedDate.Date);
                    }
                }
                else
                {
                    var current = _authService.GetCurrentUser();
                    if (current != null)
                    {
                        Grid = await _scheduleService.GetScheduleGridAsync(current.staff_ID, SelectedDate.Date);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading schedule: {ex.Message}");
                Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
            }
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
