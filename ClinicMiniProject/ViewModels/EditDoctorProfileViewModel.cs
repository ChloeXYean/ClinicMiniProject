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

        // Availability Properties
        private bool isMon; public bool IsMon { get => isMon; set { isMon = value; OnPropertyChanged(); } }
        private bool isTue; public bool IsTue { get => isTue; set { isTue = value; OnPropertyChanged(); } }
        private bool isWed; public bool IsWed { get => isWed; set { isWed = value; OnPropertyChanged(); } }
        private bool isThu; public bool IsThu { get => isThu; set { isThu = value; OnPropertyChanged(); } }
        private bool isFri; public bool IsFri { get => isFri; set { isFri = value; OnPropertyChanged(); } }
        private bool isSat; public bool IsSat { get => isSat; set { isSat = value; OnPropertyChanged(); } }
        private bool isSun; public bool IsSun { get => isSun; set { isSun = value; OnPropertyChanged(); } }

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

            // Fire and forget load
            _ = LoadCurrentDoctorProfile();
        }

        private async Task LoadCurrentDoctorProfile()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return;

            try
            {
                // Fetch full profile including availability from database
                var profile = await _doctorProfileService.GetDoctorProfileAsync(user.staff_ID);

                if (profile != null)
                {
                    Name = profile.Name;
                    PhoneNumber = profile.PhoneNo;
                    WorkingHours = profile.WorkingHoursText;

                    if (!string.IsNullOrEmpty(profile.ProfileImageUri))
                        ProfilePictureSource = ImageSource.FromFile(profile.ProfileImageUri);
                    else
                        ProfilePictureSource = ImageSource.FromFile("profilepicture.png");

                    // Set Availability
                    IsMon = profile.IsMon;
                    IsTue = profile.IsTue;
                    IsWed = profile.IsWed;
                    IsThu = profile.IsThu;
                    IsFri = profile.IsFri;
                    IsSat = profile.IsSat;
                    IsSun = profile.IsSun;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
            }
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
                    ProfileImageUri = NewProfilePicturePath,
                    // Pass availability to DTO
                    IsMon = IsMon,
                    IsTue = IsTue,
                    IsWed = IsWed,
                    IsThu = IsThu,
                    IsFri = IsFri,
                    IsSat = IsSat,
                    IsSun = IsSun
                };

                await _doctorProfileService.UpdateDoctorProfileAsync(doctor.staff_ID, update);

                await Shell.Current.DisplayAlert("Success", "Profile and availability updated successfully!", "OK");
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
}