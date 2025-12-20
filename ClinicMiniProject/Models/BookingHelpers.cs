using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System;

namespace ClinicMiniProject.Models
{
    // Helper for Calendar Days
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

        // Colors
        public Color BackgroundColor => IsSelected ? Color.FromArgb("#5FA8FF") : Colors.Transparent;
        public Color TextColor => IsSelected ? Colors.White : (IsCurrentMonth ? Colors.Black : Colors.Gray);
    }

    // Helper for Time Slots
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
                }
            }
        }

        public Color BackgroundColor => IsSelected ? Color.FromArgb("#5FA8FF") : Colors.White;
        public Color TextColor => IsSelected ? Colors.White : Color.FromArgb("#5FA8FF");
        public double Opacity => IsEnabled ? 1.0 : 0.5;
    }
}