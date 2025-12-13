using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public sealed class AppointmentScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IAppointmentScheduleService _scheduleService;

        private DateTime _selectedDate = DateTime.Today;
        private AppointmentScheduleGridDto _grid = new() { Date = DateTime.Today };

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppointmentScheduleViewModel(IAuthService authService, IAppointmentScheduleService scheduleService)
        {
            _authService = authService;
            _scheduleService = scheduleService;

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

        public ICommand ViewPatientDetailsCommand { get; }

        private async void LoadSchedule()
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
