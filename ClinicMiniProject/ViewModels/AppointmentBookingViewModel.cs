using ClinicMiniProject.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class AppointmentBookingViewModel : BindableObject
    {
        private DateTime _currentMonth;
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentMonthName));
            }
        }

        public string CurrentMonthName => CurrentMonth.ToString("MMMM yyyy");

        private ObservableCollection<DayModel> _days;
        public ObservableCollection<DayModel> Days
        {
            get => _days;
            set
            {
                _days = value;
                OnPropertyChanged();
            }
        }

        private DayModel? _selectedDate;
        public DayModel? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _serviceTypes;
        public ObservableCollection<string> ServiceTypes
        {
            get => _serviceTypes;
            set
            {
                _serviceTypes = value;
                OnPropertyChanged();
            }
        }

        private string _selectedService = string.Empty;
        public string SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TimeSlotModel> _timeSlots;
        public ObservableCollection<TimeSlotModel> TimeSlots
        {
            get => _timeSlots;
            set
            {
                _timeSlots = value;
                OnPropertyChanged();
            }
        }

        private TimeSlotModel? _selectedTime;
        public TimeSlotModel? SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
            }
        }

        public ICommand NextMonthCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand SelectDateCommand { get; }
        public ICommand SelectTimeCommand { get; }
        public ICommand ConfirmSelectionCommand { get; }

        public static readonly TimeSpan StartTime = new TimeSpan(10, 0, 0); // 10 AM
        public static readonly TimeSpan EndTime = new TimeSpan(22, 0, 0);   // 10 PM

        public AppointmentBookingViewModel()
        {
            CurrentMonth = DateTime.Today;
            Days = new ObservableCollection<DayModel>();
            TimeSlots = new ObservableCollection<TimeSlotModel>();
            ServiceTypes = new ObservableCollection<string>(Enum.GetNames(typeof(ServiceType)));

            NextMonthCommand = new Command(NextMonth);
            PreviousMonthCommand = new Command(PreviousMonth);
            SelectDateCommand = new Command<DayModel>(SelectDate);
            SelectTimeCommand = new Command<TimeSlotModel>(SelectTime);
            ConfirmSelectionCommand = new Command(async () => await ConfirmSelection());

            GenerateCalendar();
            GenerateTimeSlots();
        }

        private void GenerateCalendar()
        {
            Days.Clear();

            // Get the first day of the month
            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);

            // Find the starting day of the week (Sunday)
            var startDay = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

            var today = DateTime.Today;

            // Generate 42 days (6 weeks) to cover any month view
            for (int i = 0; i < 42; i++)
            {
                var date = startDay.AddDays(i);
                var dayModel = new DayModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == CurrentMonth.Month
                };

                // Disable dates before today
                if (date.Date < today)
                {
                    dayModel.IsEnabled = false;
                }
                else
                {
                    dayModel.IsEnabled = true;
                }

                Days.Add(dayModel);
            }
        }

        private void GenerateTimeSlots()
        {
            TimeSlots.Clear();
            var start = StartTime;
            var end = EndTime;

            while (start <= end)
            {
                // Initial generation, validity checked when date is selected
                TimeSlots.Add(new TimeSlotModel { Time = start, IsEnabled = true });
                start = start.Add(TimeSpan.FromHours(1));
            }
        }

        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            GenerateCalendar();
        }

        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            GenerateCalendar();
        }

        private void SelectDate(DayModel day)
        {
            if (day == null) return;
            if (!day.IsEnabled) return; // Prevent selection of disabled days

            if (SelectedDate != null)
            {
                SelectedDate.IsSelected = false;
            }

            SelectedDate = day;
            SelectedDate.IsSelected = true;

            UpdateTimeSlotsAvailability();
        }

        private void UpdateTimeSlotsAvailability()
        {
            if (SelectedDate == null) return;

            var now = DateTime.Now;
            var isToday = SelectedDate.Date == now.Date;

            foreach (var slot in TimeSlots)
            {
                if (isToday)
                {
                    // Disable if time has passed
                    // We can compare TimeSlotModel.Time (TimeSpan) with now.TimeOfDay
                    // But if now is 10:30, 10:00 slot starts before now. 
                    // Usually we want to book for future. Let's strictly say start time > now
                    // Or maybe give a buffer. Let's stick to start time > now.TimeOfDay

                    slot.IsEnabled = slot.Time > now.TimeOfDay;
                }
                else
                {
                    slot.IsEnabled = true;
                }

                // If currently selected time becomes disabled, deselect it
                if (!slot.IsEnabled && slot.IsSelected)
                {
                    slot.IsSelected = false;
                    SelectedTime = null;
                }
            }
        }

        private void SelectTime(TimeSlotModel time)
        {
            if (time == null) return;
            if (!time.IsEnabled) return;

            if (SelectedTime != null)
            {
                SelectedTime.IsSelected = false;
            }

            SelectedTime = time;
            SelectedTime.IsSelected = true;
        }

        private async Task ConfirmSelection()
        {
            if (SelectedDate == null)
            {
                await Shell.Current.DisplayAlert("Error", "Please select a date.", "OK");
                return;
            }
            if (SelectedTime == null)
            {
                await Shell.Current.DisplayAlert("Error", "Please select a time.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(SelectedService))
            {
                await Shell.Current.DisplayAlert("Error", "Please select a service.", "OK");
                return;
            }

            // Create navigation parameters
            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedDate", SelectedDate.Date },
                { "SelectedTime", SelectedTime.Time },
                { "SelectedService", SelectedService }
            };

            await Shell.Current.GoToAsync("SelectDoctorPage", navigationParameter);
        }

        // Models for UI State - Manually implementing INotifyPropertyChanged or BindableObject
        public class DayModel : BindableObject
        {
            public DateTime Date { get; set; }
            public string DayNumber => Date.Day.ToString();
            public bool IsCurrentMonth { get; set; }

            private bool _isEnabled;
            public bool IsEnabled
            {
                get => _isEnabled;
                set
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TextColor));
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }

            public Color BackgroundColor => IsSelected ? Color.FromArgb("#5FA8FF") : Colors.Transparent;

            public Color TextColor
            {
                get
                {
                    if (IsSelected) return Colors.White;
                    if (!IsEnabled) return Colors.LightGray; // Visibly disabled
                    return IsCurrentMonth ? Colors.Black : Colors.Gray;
                }
            }
        }

        public class TimeSlotModel : BindableObject
        {
            public TimeSpan Time { get; set; }
            public string DisplayTime => DateTime.Today.Add(Time).ToString("h:mm tt"); // e.g. 10:00 AM

            private bool _isEnabled;
            public bool IsEnabled
            {
                get => _isEnabled;
                set
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                    OnPropertyChanged(nameof(Opacity));
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }

            public double Opacity => IsEnabled ? 1.0 : 0.5;
            public Color BackgroundColor => IsSelected ? Color.FromArgb("#5FA8FF") : (IsEnabled ? Colors.White : Colors.WhiteSmoke);
            public Color TextColor => IsSelected ? Colors.White : (IsEnabled ? Colors.Black : Colors.LightGray);
        }
    }
}
