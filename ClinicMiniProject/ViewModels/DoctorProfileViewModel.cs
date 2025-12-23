using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Storage;

namespace ClinicMiniProject.ViewModels
{
    public class DoctorProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IDoctorProfileService _doctorProfileService;

        private string _doctorId = string.Empty;
        private string _name = string.Empty;
        private string _phoneNo = string.Empty;
        private string _workingHoursText = string.Empty;
        private string _profileImageUri = string.Empty;
        private bool _isEditMode;

        public string DoctorId
        {
            get => _doctorId;
            set => SetProperty(ref _doctorId, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(DoctorName));
                }
            }
        }

        public string DoctorName
        {
            get => Name;
            set => Name = value;
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

        public string PhoneNumber
        {
            get => PhoneNo;
            set => PhoneNo = value;
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

        public string WorkingHours
        {
            get => WorkingHoursText;
            set => WorkingHoursText = value;
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
                    OnPropertyChanged(nameof(IsReadOnlyMode));
                }
            }
        }

        public bool IsReadOnlyMode => !IsEditMode;

        public ObservableCollection<ServiceItem> ServicesProvided { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand ToggleEditModeCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DoctorProfileViewModel(IAuthService authService, IDoctorProfileService doctorProfileService)
        {
            _authService = authService;
            _doctorProfileService = doctorProfileService;

            RefreshCommand = new Command(async () => await RefreshAsync());
            SaveProfileCommand = new Command(async () => await SaveAsync());
            ToggleEditModeCommand = new Command(OnToggleEditMode);
            ChangeProfilePictureCommand = new Command(async () => await ChangeProfilePictureAsync());
            AddServiceCommand = new Command(async () => await AddServiceAsync());
            RemoveServiceCommand = new Command<ServiceItem>(OnRemoveService);
            EditProfileCommand = new Command(async () => await Shell.Current.GoToAsync("EditDoctorProfile"));
            LogoutCommand = new Command(async () =>
            {
                _authService.Logout();
                await Shell.Current.GoToAsync($"///LoginPage");
            });
        }

        public async Task RefreshAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== RefreshAsync Started ===");
            
            var doctor = _authService.GetCurrentUser();
            System.Diagnostics.Debug.WriteLine($"RefreshAsync - Current user: {doctor?.staff_ID}, Name: {doctor?.staff_name}, Contact: {doctor?.staff_contact}");
            
            if (doctor == null)
            {
                System.Diagnostics.Debug.WriteLine("No current user found - user might not be logged in");
                await Shell.Current.DisplayAlert("Error", "No user logged in", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Attempting to load profile for doctor ID: {doctor.staff_ID}");
            
            var dto = await _doctorProfileService.GetDoctorProfileAsync(doctor.staff_ID);
            System.Diagnostics.Debug.WriteLine($"GetDoctorProfileAsync result: {dto?.DoctorId}, Name: {dto?.Name}, Phone: {dto?.PhoneNo}, WorkingHours: {dto?.WorkingHoursText}");
            
            if (dto == null)
            {
                System.Diagnostics.Debug.WriteLine("DoctorProfileDto returned null - service might not be working");
                await Shell.Current.DisplayAlert("Error", "Failed to load profile", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Setting DoctorId to: {dto.DoctorId}");
            DoctorId = dto.DoctorId;
            
            System.Diagnostics.Debug.WriteLine($"Setting Name to: {dto.Name}");
            Name = dto.Name;
            
            System.Diagnostics.Debug.WriteLine($"Setting PhoneNo to: {dto.PhoneNo}");
            PhoneNo = dto.PhoneNo;
            
            // Set default working hours to 9am-9pm if not set
            var workingHours = string.IsNullOrEmpty(dto.WorkingHoursText) ? "9:00 AM - 9:00 PM" : dto.WorkingHoursText;
            System.Diagnostics.Debug.WriteLine($"Setting WorkingHoursText to: {workingHours}");
            WorkingHoursText = workingHours;
            
            System.Diagnostics.Debug.WriteLine($"Setting ProfileImageUri to: {dto.ProfileImageUri}");
            ProfileImageUri = dto.ProfileImageUri;

            ServicesProvided.Clear();
            foreach (var s in dto.ServicesProvided)
                ServicesProvided.Add(new ServiceItem { Name = s });
                
            System.Diagnostics.Debug.WriteLine($"Profile loaded successfully:");
            System.Diagnostics.Debug.WriteLine($"  DoctorId: {DoctorId}");
            System.Diagnostics.Debug.WriteLine($"  Name: {Name}");
            System.Diagnostics.Debug.WriteLine($"  DoctorName: {DoctorName}");
            System.Diagnostics.Debug.WriteLine($"  PhoneNo: {PhoneNo}");
            System.Diagnostics.Debug.WriteLine($"  PhoneNumber: {PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"  WorkingHoursText: {WorkingHoursText}");
            System.Diagnostics.Debug.WriteLine($"  WorkingHours: {WorkingHours}");
            System.Diagnostics.Debug.WriteLine($"  Services count: {ServicesProvided.Count}");
            System.Diagnostics.Debug.WriteLine("=== RefreshAsync Completed ===");
        }

        public async Task SaveAsync()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

            try
            {
                var update = new DoctorProfileUpdateDto
                {
                    Name = Name,
                    PhoneNo = PhoneNo,
                    WorkingHoursText = WorkingHoursText,
                    ServicesProvided = ServicesProvided.Select(s => s.Name).ToList(),
                    ProfileImageUri = ProfileImageUri
                };

                await _doctorProfileService.UpdateDoctorProfileAsync(doctor.staff_ID, update);

                IsEditMode = false;
                
                await Shell.Current.DisplayAlert("Success", "Profile updated successfully and saved to database!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save profile: {ex.Message}", "OK");
            }
        }

        private void OnToggleEditMode()
        {
            IsEditMode = !IsEditMode;
        }

        private async Task ChangeProfilePictureAsync()
        {
            if (!IsEditMode)
                return;

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select profile picture"
            });

            if (result != null)
                ProfileImageUri = result.FullPath;
        }

        private async Task AddServiceAsync()
        {
            if (!IsEditMode)
                return;

            var name = await Shell.Current.DisplayPromptAsync("Add Service", "Service name:");
            if (string.IsNullOrWhiteSpace(name))
                return;

            ServicesProvided.Add(new ServiceItem { Name = name.Trim() });
        }

        private void OnRemoveService(ServiceItem item)
        {
            if (!IsEditMode)
                return;

            if (item == null)
                return;

            ServicesProvided.Remove(item);
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class ServiceItem
    {
        public string Name { get; set; } = string.Empty;
    }
}
