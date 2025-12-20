using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI.Nurse; // Ensure namespace for NurseHomePage reference if needed
using Microsoft.Maui.Storage;

namespace ClinicMiniProject.ViewModels
{
    public class NurseProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly INurseProfileService _nurseProfileService;

        // ... [Keep existing properties: NurseId, Name, PhoneNo, etc.] ...
        private string _nurseId = string.Empty;
        private string _name = string.Empty;
        private string _phoneNo = string.Empty;
        private string _workingHoursText = string.Empty;
        private string _department = string.Empty;
        private string _profileImageUri = string.Empty;
        private bool _isEditMode;

        public string NurseId { get => _nurseId; set => SetProperty(ref _nurseId, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string NurseName { get => Name; set => Name = value; }
        public string PhoneNo { get => _phoneNo; set => SetProperty(ref _phoneNo, value); }
        public string PhoneNumber { get => PhoneNo; set => PhoneNo = value; }
        public string WorkingHoursText { get => _workingHoursText; set => SetProperty(ref _workingHoursText, value); }
        public string WorkingHours { get => WorkingHoursText; set => WorkingHoursText = value; }
        public string Department { get => _department; set => SetProperty(ref _department, value); }
        public string ProfileImageUri { get => _profileImageUri; set => SetProperty(ref _profileImageUri, value); }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { if (SetProperty(ref _isEditMode, value)) OnPropertyChanged(nameof(IsReadOnlyMode)); }
        }
        public bool IsReadOnlyMode => !IsEditMode;

        // Navigation Commands
        public ICommand HomeCommand { get; }
        public ICommand InquiryCommand { get; }

        // Action Commands
        public ICommand RefreshCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand ToggleEditModeCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public NurseProfileViewModel(IAuthService authService, INurseProfileService nurseProfileService)
        {
            _authService = authService;
            _nurseProfileService = nurseProfileService;

            // --- Fix: Initialize Navigation Commands ---
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}"));
            InquiryCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));

            RefreshCommand = new Command(async () => await RefreshAsync());
            SaveProfileCommand = new Command(async () => await SaveAsync());
            ToggleEditModeCommand = new Command(OnToggleEditMode);
            ChangeProfilePictureCommand = new Command(async () => await ChangeProfilePictureAsync());

            // Logout logic
            LogoutCommand = new Command(async () =>
            {
                _authService.Logout();
                await Shell.Current.GoToAsync($"///LoginPage");
            });
        }

        // ... [Keep existing methods: RefreshAsync, SaveAsync, etc.] ...

        public async Task RefreshAsync()
        {
            var nurse = _authService.GetCurrentUser();
            if (nurse == null) return;
            var dto = await _nurseProfileService.GetNurseProfileAsync(nurse.staff_ID);
            if (dto == null) return;
            NurseId = dto.NurseId;
            Name = dto.Name;
            PhoneNo = dto.PhoneNo;
            WorkingHoursText = dto.WorkingHoursText;
            Department = dto.Department;
            ProfileImageUri = dto.ProfileImageUri;
        }

        public async Task SaveAsync()
        {
            var nurse = _authService.GetCurrentUser();
            if (nurse == null) return;
            var update = new NurseProfileUpdateDto
            {
                Name = Name,
                PhoneNo = PhoneNo,
                WorkingHoursText = WorkingHoursText,
                Department = Department,
                ProfileImageUri = ProfileImageUri
            };
            var success = await _nurseProfileService.UpdateNurseProfileAsync(nurse.staff_ID, update);
            if (success) { IsEditMode = false; await RefreshAsync(); }
            else { await Shell.Current.DisplayAlert("Error", "Failed to update profile.", "OK"); }
        }

        private void OnToggleEditMode() => IsEditMode = !IsEditMode;

        private async Task ChangeProfilePictureAsync()
        {
            if (!IsEditMode) return;
            var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select profile picture" });
            if (result != null) ProfileImageUri = result.FullPath;
        }

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