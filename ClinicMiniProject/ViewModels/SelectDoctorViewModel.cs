using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
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
                System.Diagnostics.Debug.WriteLine($"SelectedDate set to: {value}");
                OnPropertyChanged();
                // Don't call LoadDoctors here - let it be called manually for testing
            }
        }

        private TimeSpan _selectedTime;
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                System.Diagnostics.Debug.WriteLine($"SelectedTime set to: {value}");
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
                System.Diagnostics.Debug.WriteLine($"SelectedService set to: {value}");
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
            try
            {
                System.Diagnostics.Debug.WriteLine("SelectDoctorViewModel constructor started");
                
                if (context == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: AppDbContext is null in constructor");
                    throw new ArgumentNullException(nameof(context));
                }
                
                _context = context;
                AvailableDoctors = new ObservableCollection<Staff>();
                SelectDoctorCommand = new Command<Staff>(async (doctor) => await SelectDoctor(doctor));
                GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
                
                System.Diagnostics.Debug.WriteLine("SelectDoctorViewModel constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SelectDoctorViewModel constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async void LoadDoctors()
        {
            System.Diagnostics.Debug.WriteLine("LoadDoctors called manually");
            
            if (SelectedDate == default) 
            {
                System.Diagnostics.Debug.WriteLine("SelectedDate is default, returning");
                return;
            }

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    AvailableDoctors.Clear();
                    System.Diagnostics.Debug.WriteLine("AvailableDoctors cleared");

                    // Just get all doctors for testing
                    var doctors = await _context.Staffs
                        .Where(s => s.isDoctor)
                        .ToListAsync();

                    System.Diagnostics.Debug.WriteLine($"Found {doctors.Count} doctors");

                    foreach (var doc in doctors)
                    {
                        AvailableDoctors.Add(doc);
                        System.Diagnostics.Debug.WriteLine($"Added doctor: {doc.staff_name}");
                    }

                    System.Diagnostics.Debug.WriteLine($"AvailableDoctors collection now has {AvailableDoctors.Count} items");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadDoctors: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load doctors: {ex.Message}", "OK");
            }
        }

        private async Task SelectDoctor(Staff doctor)
        {
            if (doctor == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Booking",
                $"Book appointment with {doctor.staff_name}?",
                "Yes", "No");

            if (confirm)
            {
                await Shell.Current.DisplayAlert("Success", "Appointment Request Sent!", "OK");
                await Shell.Current.GoToAsync("///PatientHomePage");
            }
        }
    }
}