using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.ViewModels
{
    public class PatientAppointmentBookingViewModel : BindableObject
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthService _authService;

        // --- DATA COLLECTIONS ---
        public ObservableCollection<CalendarDay> Days { get; } = new();
        public ObservableCollection<TimeSlotItem> TimeSlots { get; } = new();
        public ObservableCollection<string> ServiceTypes { get; } = new()
        {
            "General Consultation",
            "Follow Up Treatment",
            "Vaccination",
            "Medical Checkup"
        };

        // --- PROPERTIES ---
        private DateTime _currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public string CurrentMonthName => _currentMonthStart.ToString("MMMM yyyy");

        private string _selectedService = "General Consultation";
        public string SelectedService
        {
            get => _selectedService;
            set { _selectedService = value; OnPropertyChanged(); }
        }

        private DateTime _selectedDate = DateTime.Today;

        // --- COMMANDS ---
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand SelectDateCommand { get; }
        public ICommand SelectTimeCommand { get; }
        public ICommand ConfirmSelectionCommand { get; } // Now navigates to Doctor Selection
        public ICommand BackCommand { get; }

        // --- CONSTRUCTOR ---
        public PatientAppointmentBookingViewModel(IAppointmentService appointmentService, IAuthService authService)
        {
            _appointmentService = appointmentService;
            _authService = authService;

            PreviousMonthCommand = new Command(() => ChangeMonth(-1));
            NextMonthCommand = new Command(() => ChangeMonth(1));
            SelectDateCommand = new Command<CalendarDay>(OnDateSelected);
            SelectTimeCommand = new Command<TimeSlotItem>(OnTimeSelected);

            // FIX: This now navigates instead of saving
            ConfirmSelectionCommand = new Command(async () => await OnNavigateToDoctorSelection());

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            GenerateCalendar();
            _ = GenerateTimeSlots(); // Fire and forget for initial load
        }

        // --- CALENDAR LOGIC ---
        private void ChangeMonth(int months)
        {
            _currentMonthStart = _currentMonthStart.AddMonths(months);
            OnPropertyChanged(nameof(CurrentMonthName));
            GenerateCalendar();
        }

        private void GenerateCalendar()
        {
            Days.Clear();
            int daysInMonth = DateTime.DaysInMonth(_currentMonthStart.Year, _currentMonthStart.Month);
            int startDayOffset = (int)_currentMonthStart.DayOfWeek;

            // Pad previous month
            var prevMonth = _currentMonthStart.AddMonths(-1);
            int daysInPrev = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            for (int i = 0; i < startDayOffset; i++)
            {
                Days.Add(new CalendarDay
                {
                    Date = new DateTime(prevMonth.Year, prevMonth.Month, daysInPrev - startDayOffset + 1 + i),
                    IsCurrentMonth = false,
                    IsEnabled = false
                });
            }

            // Current month
            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(_currentMonthStart.Year, _currentMonthStart.Month, i);
                Days.Add(new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = true,
                    IsEnabled = date.Date >= DateTime.Today,
                    IsSelected = date.Date == _selectedDate.Date
                });
            }

            // Pad next month
            int remaining = 42 - Days.Count;
            var nextMonth = _currentMonthStart.AddMonths(1);
            for (int i = 1; i <= remaining; i++)
            {
                Days.Add(new CalendarDay
                {
                    Date = new DateTime(nextMonth.Year, nextMonth.Month, i),
                    IsCurrentMonth = false,
                    IsEnabled = false
                });
            }
        }

        private async void OnDateSelected(CalendarDay day)
        {
            if (day == null || !day.IsEnabled) return;

            foreach (var d in Days) d.IsSelected = false;
            day.IsSelected = true;
            _selectedDate = day.Date;
            await GenerateTimeSlots(); // Refresh time slots for the new date
        }

        // --- TIME SLOT LOGIC ---
        private async Task GenerateTimeSlots()
        {
            TimeSlots.Clear();
            var startTime = new TimeSpan(9, 0, 0);
            var endTime = new TimeSpan(17, 0, 0);
            var now = DateTime.Now;

            // Get current patient's IC
            var currentPatient = _authService.GetCurrentPatient();
            if (currentPatient == null) return;

            // Get all time slots this patient has booked on the selected date (across all doctors)
            var patientBookedSlots = await _appointmentService.GetPatientBookedTimeSlotsForDateAsync(
                currentPatient.patient_IC, _selectedDate);

            // FIX: Get all unavailable slots in a SINGLE query to avoid DbContext threading issues
            var unavailableSlots = await _appointmentService.GetUnavailableTimeSlotsForDateAsync(_selectedDate);

            while (startTime < endTime)
            {
                bool isPast = _selectedDate.Date == DateTime.Today && startTime < now.TimeOfDay;
                bool isBookedByPatient = patientBookedSlots.Contains(startTime);
                bool allDoctorsBooked = unavailableSlots.Contains(startTime);

                TimeSlots.Add(new TimeSlotItem 
                { 
                    Time = startTime,
                    // Disable if: past time, patient already booked, OR no doctors available
                    IsEnabled = !isPast && !isBookedByPatient && !allDoctorsBooked,
                    IsBookedByPatient = isBookedByPatient,
                    AllDoctorsBooked = allDoctorsBooked
                });
                startTime = startTime.Add(TimeSpan.FromHours(1));
            }
        }

        private void OnTimeSelected(TimeSlotItem slot)
        {
            if (slot == null || !slot.IsEnabled) return;
            foreach (var t in TimeSlots) t.IsSelected = false;
            slot.IsSelected = true;
        }

        // --- NAVIGATION LOGIC ---
        private async Task OnNavigateToDoctorSelection()
        {
            var selectedTimeSlot = TimeSlots.FirstOrDefault(t => t.IsSelected);

            if (selectedTimeSlot == null)
            {
                await Shell.Current.DisplayAlert("Error", "Please select a time slot.", "OK");
                return;
            }

            // Prepare data to pass to the Select Doctor page
            var navParams = new Dictionary<string, object>
            {
                { "SelectedDate", _selectedDate },
                { "SelectedTime", selectedTimeSlot.Time },
                { "SelectedService", SelectedService }
            };

            // Navigate to SelectDoctorPage
            await Shell.Current.GoToAsync("SelectDoctorPage", navParams);
        }
    }

    // --- HELPER CLASSES (Included here to prevent missing errors) ---
    public class CalendarDay : BindableObject
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();
        public bool IsEnabled { get; set; } = true;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }
        }
        public bool IsCurrentMonth { get; set; }
        public Color BackgroundColor => IsSelected ? Color.FromArgb("#5FA8FF") : Colors.Transparent;
        public Color TextColor
        {
            get
            {
                if (IsSelected) return Colors.White;
                if (!IsCurrentMonth || !IsEnabled) return Colors.Gray;
                return Colors.Black;
            }
        }
    }

    public class TimeSlotItem : BindableObject
    {
        public TimeSpan Time { get; set; }
        public string DisplayTime => DateTime.Today.Add(Time).ToString("hh:mm tt");

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                    OnPropertyChanged(nameof(Opacity));
                }
            }
        }
        
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Opacity));
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }
        }
        
        private bool _isBookedByPatient;
        public bool IsBookedByPatient
        {
            get => _isBookedByPatient;
            set
            {
                if (_isBookedByPatient != value)
                {
                    _isBookedByPatient = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }
        }
        
        private bool _allDoctorsBooked;
        public bool AllDoctorsBooked
        {
            get => _allDoctorsBooked;
            set
            {
                if (_allDoctorsBooked != value)
                {
                    _allDoctorsBooked = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color BackgroundColor
        {
            get
            {
                if (IsSelected) return Color.FromArgb("#5FA8FF");
                if (!IsEnabled) return Color.FromArgb("#F0F0F0"); // Light gray for disabled
                return Colors.White;
            }
        }
        
        public Color TextColor
        {
            get
            {
                if (IsSelected) return Colors.White;
                if (!IsEnabled) return Color.FromArgb("#CCCCCC"); // Lighter gray for disabled text
                return Color.FromArgb("#5FA8FF");
            }
        }
        
        public double Opacity => IsEnabled ? 1.0 : 0.5;
    }
}