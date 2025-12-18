using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(SelectedDate), "SelectedDate")]
    [QueryProperty(nameof(SelectedTime), "SelectedTime")]
    [QueryProperty(nameof(SelectedService), "SelectedService")]
    public class SelectDoctorViewModel : BindableObject
    {
        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                Task.Run(LoadDoctors); // Trigger load when property is set by QueryProperty
            }
        }

        private TimeSpan _selectedTime;
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
                // LoadDoctors will be triggered by Date set, but if only Time changes...
                // Usually QueryProperty sets them one by one. Calling LoadDoctors twice is fine or manage state.
            }
        }

        private string _selectedService;
        public string SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Staff> _availableDoctors;
        public ObservableCollection<Staff> AvailableDoctors
        {
            get => _availableDoctors;
            set
            {
                _availableDoctors = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectDoctorCommand { get; }
        public ICommand GoBackCommand { get; }

        private AppDbContext? _dbContext; 

        public SelectDoctorViewModel()
        {
            AvailableDoctors = new ObservableCollection<Staff>();
            SelectDoctorCommand = new Command<Staff>(async (doctor) => await SelectDoctor(doctor));
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }
        
        public SelectDoctorViewModel(AppDbContext dbContext) : this()
        {
            _dbContext = dbContext;
        }

        public async Task LoadDoctors()
        {
            // Delay slightly to ensure both QueryProperties are set (Date and Time)
            // Or just check validity
            if (SelectedDate == default) return;
            
            if (_dbContext == null)
            {
               var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
               // Note: Ideally we get connection string from elsewhere, but failing that we rely on "OnConfiguring" internals 
               // or we instantiate it such that it configures itself.
               _dbContext = new AppDbContext(optionsBuilder.Options);
            }

            // Need to marshal to MainThread if modifying ObservableCollection from background task
            await MainThread.InvokeOnMainThreadAsync(async () => 
            {
                AvailableDoctors.Clear();

                DayOfWeek dayOfWeek = SelectedDate.DayOfWeek;

                var doctors = await _dbContext.Staffs
                    .Include(s => s.Availability)
                    .Where(s => s.isDoctor)
                    .ToListAsync();

                var available = new List<Staff>();

                foreach (var doc in doctors)
                {
                    bool isDayAvailable = false;
                    if (doc.Availability != null)
                    {
                        isDayAvailable = doc.Availability.IsAvailable(dayOfWeek);
                    }

                    if (!isDayAvailable) continue;

                    DateTime appointmentDateTime = SelectedDate.Date + SelectedTime;
                    
                    var conflict = await _dbContext.Appointments
                        .AnyAsync(a => a.staff_ID == doc.staff_ID 
                                       && a.appointedAt == appointmentDateTime 
                                       && a.status != "Cancelled");

                    if (!conflict)
                    {
                        available.Add(doc);
                    }
                }

                foreach (var doc in available)
                {
                    AvailableDoctors.Add(doc);
                }
            });
        }

        private async Task SelectDoctor(Staff doctor)
        {
            if (doctor == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Booking", 
                $"Book appointment with {doctor.staff_name} on {SelectedDate:d} at {DateTime.Today.Add(SelectedTime):h:mm tt}?", 
                "Yes", "No");

            if (confirm)
            {
                // TODO: Save to DB
                // var newAppointment = ... 
                
                await Shell.Current.DisplayAlert("Success", "Appointment Request Sent!", "OK");
                await Shell.Current.GoToAsync("///PatientHomePage");
            }
        }
    }
}
