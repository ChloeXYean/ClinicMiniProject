using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceScopeFactory

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(SelectedDate), "SelectedDate")]
    [QueryProperty(nameof(SelectedTime), "SelectedTime")]
    [QueryProperty(nameof(SelectedService), "SelectedService")]
    public class SelectDoctorViewModel : BindableObject
    {
        // Replace direct DbContext with ScopeFactory to create fresh contexts per thread
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAuthService _authService;
        private readonly IAppointmentService _appointmentService;

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                // This fire-and-forget call is now safe because it will create its own Context
                _ = LoadDoctorsAsync();
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

        // Inject IServiceScopeFactory instead of AppDbContext
        public SelectDoctorViewModel(IServiceScopeFactory scopeFactory, IAuthService authService, IAppointmentService appointmentService)
        {
            _scopeFactory = scopeFactory;
            _authService = authService;
            _appointmentService = appointmentService;

            AvailableDoctors = new ObservableCollection<Staff>();
            SelectDoctorCommand = new Command<Staff>(async (doctor) => await SelectDoctor(doctor));
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task LoadDoctorsAsync()
        {
            if (SelectedDate == default) return;

            try
            {
                var dayOfWeek = SelectedDate.DayOfWeek;
                var appointmentDateTime = SelectedDate.Date + SelectedTime;

                // Create a new scope for this specific execution to avoid Threading conflicts
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Do DB work on background thread using the fresh context
                    var doctors = await context.Staffs
                        .Include(s => s.Availability)
                        .Where(s => s.isDoctor)
                        .ToListAsync();

                    var available = new List<Staff>();
                    foreach (var doc in doctors)
                    {
                        if (doc.Availability == null) continue;
                        if (!doc.Availability.IsAvailable(dayOfWeek)) continue;

                        bool conflict = await context.Appointments
                            .AnyAsync(a => a.staff_ID == doc.staff_ID
                                           && a.appointedAt == appointmentDateTime
                                           && a.status != "Cancelled");

                        if (!conflict)
                        {
                            available.Add(doc);
                        }
                    }

                    // Update UI on Main thread
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        AvailableDoctors.Clear();
                        foreach (var doc in available)
                        {
                            AvailableDoctors.Add(doc);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading doctors: {ex.Message}");
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
                try
                {
                    var patient = _authService.GetCurrentPatient();
                    if (patient == null)
                    {
                        await Shell.Current.DisplayAlert("Error", "You must be logged in as a patient to book an appointment.", "OK");
                        return;
                    }

                    // Create the appointment object
                    var newAppointment = new Appointment
                    {
                        patient_IC = patient.patient_IC,
                        staff_ID = doctor.staff_ID,
                        appointedAt = SelectedDate.Date + SelectedTime,
                        bookedAt = DateTime.Now,
                        status = "Pending",
                        service_type = SelectedService ?? "General Consultation"
                    };

                    bool success = await _appointmentService.AddAppointmentAsync(newAppointment);

                    if (success)
                    {

                        await Shell.Current.DisplayAlert("Success", "Appointment Request Sent!", "OK");
                        await Shell.Current.GoToAsync("///PatientHomePage");
                    }
                }
                catch (Exception ex)
                {
                    var fullMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        fullMessage += $"\nInner Error: {ex.InnerException.Message}";
                    }
                    System.Diagnostics.Debug.WriteLine($"General error: {fullMessage}");
                    await Shell.Current.DisplayAlert("Error", $"Failed to create appointment: {fullMessage}", "OK");
                }
            }
        }
    }
}