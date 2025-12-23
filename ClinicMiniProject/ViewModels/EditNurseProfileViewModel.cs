using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Dtos;

namespace ClinicMiniProject.ViewModels
{
    public class EditNurseProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly INurseProfileService _nurseProfileService;

        private string name = string.Empty;
        public string Name { get => name; set { name = value; OnPropertyChanged(); } }

        private string phoneNumber = string.Empty;
        public string PhoneNumber { get => phoneNumber; set { phoneNumber = value; OnPropertyChanged(); } }

        private string department = string.Empty;
        public string Department { get => department; set { department = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditNurseProfileViewModel(IAuthService authService, INurseProfileService nurseProfileService)
        {
            _authService = authService;
            _nurseProfileService = nurseProfileService;

            SaveCommand = new Command(async () => await OnSave());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            LoadCurrentProfile();
        }

        private async void LoadCurrentProfile()
        {
            var nurse = _authService.GetCurrentUser();
            if (nurse == null) return;

            var dto = await _nurseProfileService.GetNurseProfileAsync(nurse.staff_ID);
            if (dto != null)
            {
                Name = dto.Name;
                PhoneNumber = dto.PhoneNo;
                Department = dto.Department;
            }
        }

        private async Task OnSave()
        {
            var nurse = _authService.GetCurrentUser();
            if (nurse == null) return;

            var update = new NurseProfileUpdateDto
            {
                Name = Name,
                PhoneNo = PhoneNumber,
                Department = Department
            };

            var success = await _nurseProfileService.UpdateNurseProfileAsync(nurse.staff_ID, update);
            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Profile updated successfully!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update profile.", "OK");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}