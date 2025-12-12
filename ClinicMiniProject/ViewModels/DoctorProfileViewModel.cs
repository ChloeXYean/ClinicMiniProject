using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

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

        public string PhoneNo
        {
            get => _phoneNo;
            set => SetProperty(ref _phoneNo, value);
        }

        public string WorkingHoursText
        {
            get => _workingHoursText;
            set => SetProperty(ref _workingHoursText, value);
        }

        public string ProfileImageUri
        {
            get => _profileImageUri;
            set => SetProperty(ref _profileImageUri, value);
        }

        public ObservableCollection<string> ServicesProvided { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SaveCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DoctorProfileViewModel(IAuthService authService, IDoctorProfileService doctorProfileService)
        {
            _authService = authService;
            _doctorProfileService = doctorProfileService;

            RefreshCommand = new Command(async () => await RefreshAsync());
            SaveCommand = new Command(async () => await SaveAsync());
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
            WorkingHoursText = dto.WorkingHoursText;
            ProfileImageUri = dto.ProfileImageUri;

            ServicesProvided.Clear();
            foreach (var s in dto.ServicesProvided)
                ServicesProvided.Add(s);
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
                ServicesProvided = ServicesProvided,
                ProfileImageUri = ProfileImageUri
            };

            await _doctorProfileService.UpdateDoctorProfileAsync(doctor.staff_ID, update);
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
}
