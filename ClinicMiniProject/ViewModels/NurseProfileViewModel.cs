using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI.Nurse; // Needed for NurseHomePage reference
using Microsoft.Maui.Storage;

namespace ClinicMiniProject.ViewModels
{
    public class NurseProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly INurseProfileService _nurseProfileService;

        // Private backing fields
        private string _nurseId = string.Empty;
        private string _name = string.Empty;
        private string _phoneNo = string.Empty;
        private string _workingHoursText = string.Empty;
        private string _department = string.Empty;
        private string _profileImageUri = string.Empty;
        private bool _isEditMode;

        // --- Properties bound to XAML ---

        public string NurseId
        {
            get => _nurseId;
            set => SetProperty(ref _nurseId, value);
        }

        // Wrapper for Name to match XAML binding {Binding NurseName}
        public string NurseName
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(Name)); // Notify both properties
                }
            }
        }

        // Direct property (optional, but good for internal use)
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(NurseName));
                }
            }
        }

        // Wrapper for PhoneNo to match XAML binding {Binding PhoneNumber}
        public string PhoneNumber
        {
            get => _phoneNo;
            set
            {
                if (SetProperty(ref _phoneNo, value))
                {
                    OnPropertyChanged(nameof(PhoneNo));
                }
            }
        }

        public string PhoneNo
        {
            get => _phoneNo;
            set
            {
                if (SetProperty(ref _phoneNo, value))
                {
                    OnPropertyChanged(nameof(PhoneNumber));
                }
            }
        }

        // Wrapper for WorkingHours to match XAML binding {Binding WorkingHours}
        public string WorkingHours
        {
            get => _workingHoursText;
            set
            {
                if (SetProperty(ref _workingHoursText, value))
                {
                    OnPropertyChanged(nameof(WorkingHoursText));
                }
            }
        }

        public string WorkingHoursText
        {
            get => _workingHoursText;
            set
            {
                if (SetProperty(ref _workingHoursText, value))
                {
                    OnPropertyChanged(nameof(WorkingHours));
                }
            }
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public string ProfileImageUri
        {
            get => _profileImageUri;
            set => SetProperty(ref _profileImageUri, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    OnPropertyChanged(nameof(IsReadOnlyMode)); // Notify inverse property
                }
            }
        }

        public bool IsReadOnlyMode => !IsEditMode;

        // --- Commands ---
        public ICommand HomeCommand { get; }
        public ICommand InquiryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        // --- Constructor ---
        public NurseProfileViewModel(IAuthService authService, INurseProfileService nurseProfileService)
        {
            _authService = authService;
            _nurseProfileService = nurseProfileService;

            // Navigation Commands
            // Using "///" ensures we switch tabs properly instead of pushing a new page on the stack
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}"));
            InquiryCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));

            // Functional Commands
            RefreshCommand = new Command(async () => await RefreshAsync());
            SaveProfileCommand = new Command(async () => await SaveAsync());
            EditProfileCommand = new Command(async () => await Shell.Current.GoToAsync("EditNurseProfile")); 
            ChangeProfilePictureCommand = new Command(async () => await ChangeProfilePictureAsync());

            // Logout Logic
            LogoutCommand = new Command(async () =>
            {
                _authService.Logout();
                await Shell.Current.GoToAsync("///LoginPage");
            });
        }

        // --- Methods ---

        public async Task RefreshAsync()
        {
            var nurse = _authService.GetCurrentUser();
            if (nurse == null) return;

            var dto = await _nurseProfileService.GetNurseProfileAsync(nurse.staff_ID);

            if (dto != null)
            {
                NurseId = dto.NurseId;
                Name = dto.Name;             
                PhoneNo = dto.PhoneNo;       
                Department = dto.Department;
                ProfileImageUri = dto.ProfileImageUri;
            }
        }

        public async Task SaveAsync()
        {
            var nurse = _authService.GetCurrentUser();
            if (nurse == null) return;

            var update = new NurseProfileUpdateDto
            {
                Name = Name,
                PhoneNo = PhoneNo,
                Department = Department,
                ProfileImageUri = ProfileImageUri,
                ICNumber = string.Empty // Default value as we aren't using this in UI yet
            };

            var success = await _nurseProfileService.UpdateNurseProfileAsync(nurse.staff_ID, update);

            if (success)
            {
                IsEditMode = false;
                await RefreshAsync(); // Refresh to ensure data consistency
                await Shell.Current.DisplayAlert("Success", "Profile updated successfully!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update profile.", "OK");
            }
        }

        private async Task ChangeProfilePictureAsync()
        {
            if (!IsEditMode) return;

            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select profile picture",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    ProfileImageUri = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error picking file: {ex.Message}");
            }
        }

        // --- Helper Methods ---
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}