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
            set => SetProperty(ref _name, value);
        }

        public string DoctorName
        {
            get => Name;
            set => Name = value;
        }

        public string PhoneNo
        {
            get => _phoneNo;
            set => SetProperty(ref _phoneNo, value);
        }

        public string PhoneNumber
        {
            get => PhoneNo;
            set => PhoneNo = value;
        }

        public string WorkingHoursText
        {
            get => _workingHoursText;
            set => SetProperty(ref _workingHoursText, value);
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
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

            var dto = await _doctorProfileService.GetDoctorProfileAsync(doctor.staff_ID);
            if (dto == null)
                return;

            DoctorId = dto.DoctorId;
            Name = dto.Name;
            PhoneNo = dto.PhoneNo;
            // Set default working hours to 9am-9pm if not set
            WorkingHoursText = string.IsNullOrEmpty(dto.WorkingHoursText) ? "9:00 AM - 9:00 PM" : dto.WorkingHoursText;
            ProfileImageUri = dto.ProfileImageUri;

            ServicesProvided.Clear();
            foreach (var s in dto.ServicesProvided)
                ServicesProvided.Add(new ServiceItem { Name = s });
        }

        public async Task SaveAsync()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

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
