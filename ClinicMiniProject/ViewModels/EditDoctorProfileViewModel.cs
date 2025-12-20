using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.ViewModels
{
    public class EditDoctorProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IDoctorProfileService _doctorProfileService;

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        private string phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => phoneNumber;
            set { phoneNumber = value; OnPropertyChanged(); }
        }

        private string workingHours = "9:00 AM - 9:00 PM";
        public string WorkingHours
        {
            get => workingHours;
            set { workingHours = value; OnPropertyChanged(); }
        }

        private ImageSource profilePictureSource;
        public ImageSource ProfilePictureSource
        {
            get => profilePictureSource;
            set { profilePictureSource = value; OnPropertyChanged(); }
        }

        private string newProfilePicturePath;
        public string NewProfilePicturePath
        {
            get => newProfilePicturePath;
            set { newProfilePicturePath = value; OnPropertyChanged(); }
        }

        public List<ServiceTypeItem> ServiceTypes { get; } = new()
        {
            new ServiceTypeItem { Name = "General Consultation", IsSelected = false },
            new ServiceTypeItem { Name = "Vaccination/Injection", IsSelected = false },
            new ServiceTypeItem { Name = "Follow Up Treatment", IsSelected = false },
            new ServiceTypeItem { Name = "Test Result Discussion", IsSelected = false },
            new ServiceTypeItem { Name = "Medical Checkup", IsSelected = false },
            new ServiceTypeItem { Name = "Follow-up", IsSelected = false },
            new ServiceTypeItem { Name = "Online", IsSelected = false }
        };

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditDoctorProfileViewModel(IAuthService authService, IDoctorProfileService doctorProfileService)
        {
            _authService = authService;
            _doctorProfileService = doctorProfileService;

            SaveCommand = new Command(async () => await OnSave());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ChangeProfilePictureCommand = new Command(async () => await OnChangeProfilePicture());

            LoadCurrentDoctorProfile();
        }

        private void LoadCurrentDoctorProfile()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

            // Load current profile data
            Name = doctor.staff_name ?? string.Empty;
            PhoneNumber = doctor.staff_contact ?? string.Empty;
            WorkingHours = "9:00 AM - 9:00 PM"; // Default working hours
            
            // Load profile picture
            ProfilePictureSource = ImageSource.FromFile("profilepicture.png"); // Default image
        }

        private async Task OnChangeProfilePicture()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select profile photo"
                });

                if (result != null)
                {
                    NewProfilePicturePath = result.FullPath;
                    ProfilePictureSource = ImageSource.FromFile(result.FullPath);
                    await Shell.Current.DisplayAlert("Success", "Photo selected successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to select photo: {ex.Message}", "OK");
            }
        }

        
        private async Task OnSave()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Error", "Name is required.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                await Shell.Current.DisplayAlert("Error", "Phone number is required.", "OK");
                return;
            }

            var selectedServices = ServiceTypes.Where(s => s.IsSelected).Select(s => s.Name).ToList();
            if (!selectedServices.Any())
            {
                await Shell.Current.DisplayAlert("Error", "Please select at least one service type.", "OK");
                return;
            }

            try
            {
                var doctor = _authService.GetCurrentUser();
                if (doctor == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Doctor not found.", "OK");
                    return;
                }

                var update = new DoctorProfileUpdateDto
                {
                    Name = Name,
                    PhoneNo = PhoneNumber,
                    WorkingHoursText = WorkingHours,
                    ServicesProvided = selectedServices,
                    ProfileImageUri = NewProfilePicturePath
                };

                await _doctorProfileService.UpdateDoctorProfileAsync(doctor.staff_ID, update);
                
                await Shell.Current.DisplayAlert("Success", "Profile updated successfully!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update profile: {ex.Message}", "OK");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class ServiceTypeItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
