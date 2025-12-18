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
    public sealed class AppointmentScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IAppointmentScheduleService _scheduleService;

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

        public ICommand HomeCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand ViewPatientDetailsCommand { get; }

        public AppointmentScheduleViewModel(IAuthService authService, IAppointmentScheduleService scheduleService, IStaffService staffService = null, string userType = "Doctor")
        {
            _authService = authService;
            _scheduleService = scheduleService;
            IsNurseMode = userType == "Nurse";

            if (IsNurseMode && staffService != null)
            {
                LoadDoctors(staffService);
            }

            HomeCommand = IsNurseMode 
                ? new Command(async () => await Shell.Current.GoToAsync("///NurseHomePage"))
                : new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
            ChatCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));

            ViewPatientDetailsCommand = new Command<string>(OnViewPatientDetails);
            LoadSchedule();
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
            if (IsNurseMode)
            {
                if (SelectedDoctor == null)
                {
                    Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                    return;
                }
                try
                {
                    Grid = await _scheduleService.GetScheduleGridAsync(SelectedDoctor.staff_ID, SelectedDate.Date);
                }
                catch
                {
                    Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                }
            }
            else
            {
                var current = _authService.GetCurrentUser();
                if (current == null)
                    return;

                try
                {
                    Grid = await _scheduleService.GetScheduleGridAsync(current.staff_ID, SelectedDate.Date);
                }
                catch
                {
                    Grid = new AppointmentScheduleGridDto { Date = SelectedDate.Date };
                }
            }
        }

        private void LoadDoctors(IStaffService staffService)
        {
            try
            {
                var doctorList = staffService.GetAllDocs();
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

            Shell.Current.GoToAsync($"PatientDetails?patientIc={Uri.EscapeDataString(patientIc)}");
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
