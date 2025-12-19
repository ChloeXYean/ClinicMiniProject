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
        private readonly AppDbContext _context;

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

        public SelectDoctorViewModel(AppDbContext context)
        {
            _context = context;
            AvailableDoctors = new ObservableCollection<Staff>();
            SelectDoctorCommand = new Command<Staff>(async (doctor) => await SelectDoctor(doctor));
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async void LoadDoctors()
        {
            if (SelectedDate == default) return;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    AvailableDoctors.Clear();

                    var dayOfWeek = SelectedDate.DayOfWeek;
                    var appointmentDateTime = SelectedDate.Date + SelectedTime;

                    // 2. Use the injected _context
                    var doctors = await _context.Staffs
                        .Include(s => s.Availability)
                        .Where(s => s.isDoctor)
                        .ToListAsync();

                    foreach (var doc in doctors)
                    {
                        // DEBUGGING: Check if data exists
                        if (doc.Availability == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Warning] Doctor {doc.staff_name} has NO availability set in DB.");
                            continue;
                        }

                        if (!doc.Availability.IsAvailable(dayOfWeek)) continue;

                        bool conflict = await _context.Appointments
                            .AnyAsync(a => a.staff_ID == doc.staff_ID
                                           && a.appointedAt == appointmentDateTime
                                           && a.status != "Cancelled");

                        if (!conflict)
                        {
                            AvailableDoctors.Add(doc);
                        }
                    }

                    // Alert if empty (Helps you debug)
                    if (AvailableDoctors.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No doctors found for this criteria.");
                    }
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load doctors: {ex.Message}", "OK");
            }
        }

        private async Task SelectDoctor(Staff doctor)
        {
            if (doctor == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Booking",
                $"Book appointment with {doctor.staff_name} on {SelectedDate:d} at {DateTime.Today.Add(SelectedTime):h:mm tt}?",
                "Yes", "No");

            if (confirm)
            {
                // Create the appointment object
                var newAppointment = new Appointment
                {
                    patient_IC = "123456121234", // TODO: Get logged-in user's IC
                    staff_ID = doctor.staff_ID,
                    appointedAt = SelectedDate.Date + SelectedTime,
                    bookedAt = DateTime.Now,
                    status = "Pending",
                    service_type = SelectedService ?? "General Consultation"
                };

                _context.Appointments.Add(newAppointment);
                await _context.SaveChangesAsync();

                await Shell.Current.DisplayAlert("Success", "Appointment Request Sent!", "OK");
                await Shell.Current.GoToAsync("///PatientHomePage");
            }
        }
    }
}
