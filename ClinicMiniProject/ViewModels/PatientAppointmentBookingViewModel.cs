using System.Windows.Input;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(SelectedDoctor), "SelectedDoctor")]
    public class PatientAppointmentBookingViewModel : BindableObject
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthService _authService;

        // --- PROPERTIES ---

        private Staff _selectedDoctor;
        public Staff SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                _selectedDoctor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DoctorName));
            }
        }

        public string DoctorName => SelectedDoctor?.staff_name ?? "Select a Doctor";

        private DateTime _selectedDate = DateTime.Today.AddDays(1); // Default to tomorrow
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _selectedTime = new TimeSpan(9, 0, 0); // Default 9:00 AM
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
            }
        }

        private string _serviceType = "General Consultation";
        public string ServiceType
        {
            get => _serviceType;
            set
            {
                _serviceType = value;
                OnPropertyChanged();
            }
        }

        // --- COMMANDS ---
        public ICommand ConfirmBookingCommand { get; }
        public ICommand BackCommand { get; }

        // --- CONSTRUCTOR ---
        public PatientAppointmentBookingViewModel(IAppointmentService appointmentService, IAuthService authService)
        {
            _appointmentService = appointmentService;
            _authService = authService;

            ConfirmBookingCommand = new Command(async () => await OnConfirmBooking());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        // --- ACTIONS ---
        private async Task OnConfirmBooking()
        {
            try
            {
                // 1. Validation
                if (SelectedDoctor == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No doctor selected.", "OK");
                    return;
                }

                var patient = _authService.GetCurrentPatient();
                if (patient == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Could not find patient info. Please login again.", "OK");
                    return;
                }

                // 2. Combine Date and Time
                DateTime appointmentDateTime = SelectedDate.Date + SelectedTime;

                if (appointmentDateTime < DateTime.Now)
                {
                    await Shell.Current.DisplayAlert("Error", "Cannot book an appointment in the past.", "OK");
                    return;
                }

                // 3. Create Model
                var newAppointment = new Appointment
                {
                    // Essential Foreign Keys
                    staff_ID = SelectedDoctor.staff_ID,
                    patient_IC = patient.patient_IC,

                    appointedAt = appointmentDateTime,
                    service_type = ServiceType,
                    status = "Scheduled",
                    bookedAt = DateTime.Now,
                };

                // 4. Save to Database
                await Task.Run(() => _appointmentService.AddAppointment(newAppointment));

                await Shell.Current.DisplayAlert("Success", "Appointment Booked Successfully!", "OK");

                // Navigate back to Home
                await Shell.Current.GoToAsync("///PatientHomePage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Booking Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Booking Failed", $"Error: {ex.Message}", "OK");
            }
        }
    }
}