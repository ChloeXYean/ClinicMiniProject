using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Controls;

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

        // Availability Properties (View Only)
        private bool _isMon; public bool IsMon { get => _isMon; set => SetProperty(ref _isMon, value); }
        private bool _isTue; public bool IsTue { get => _isTue; set => SetProperty(ref _isTue, value); }
        private bool _isWed; public bool IsWed { get => _isWed; set => SetProperty(ref _isWed, value); }
        private bool _isThu; public bool IsThu { get => _isThu; set => SetProperty(ref _isThu, value); }
        private bool _isFri; public bool IsFri { get => _isFri; set => SetProperty(ref _isFri, value); }
        private bool _isSat; public bool IsSat { get => _isSat; set => SetProperty(ref _isSat, value); }
        private bool _isSun; public bool IsSun { get => _isSun; set => SetProperty(ref _isSun, value); }

        public string DoctorId { get => _doctorId; set => SetProperty(ref _doctorId, value); }
        public string DoctorName { get => _name; set => SetProperty(ref _name, value); }
        public string PhoneNumber { get => _phoneNo; set => SetProperty(ref _phoneNo, value); }
        public string WorkingHours { get => _workingHoursText; set => SetProperty(ref _workingHoursText, value); }
        public string ProfileImageUri { get => _profileImageUri; set => SetProperty(ref _profileImageUri, value); }

        public ICommand RefreshCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DoctorProfileViewModel(IAuthService authService, IDoctorProfileService doctorProfileService)
        {
            _authService = authService;
            _doctorProfileService = doctorProfileService;

            RefreshCommand = new Command(async () => await RefreshAsync());

            // This now navigates to the separate Edit Page
            EditProfileCommand = new Command(async () => await Shell.Current.GoToAsync("EditDoctorProfile"));

            // Placeholder if user taps image on view page (optional, or can remove tap gesture in XAML)
            ChangeProfilePictureCommand = new Command(async () => await Shell.Current.DisplayAlert("Info", "Please click 'Edit Profile' to change your photo.", "OK"));

            LogoutCommand = new Command(async () =>
            {
                _authService.Logout();
                await Shell.Current.GoToAsync($"///LoginPage");
            });
        }

        public async Task RefreshAsync()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null) return;

            var dto = await _doctorProfileService.GetDoctorProfileAsync(doctor.staff_ID);

            if (dto == null) return;

            DoctorId = dto.DoctorId;
            DoctorName = dto.Name;
            PhoneNumber = dto.PhoneNo;
            WorkingHours = string.IsNullOrEmpty(dto.WorkingHoursText) ? "9:00 AM - 9:00 PM" : dto.WorkingHoursText;
            ProfileImageUri = dto.ProfileImageUri;

            // Map Availability for display
            IsMon = dto.IsMon;
            IsTue = dto.IsTue;
            IsWed = dto.IsWed;
            IsThu = dto.IsThu;
            IsFri = dto.IsFri;
            IsSat = dto.IsSat;
            IsSun = dto.IsSun;
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}