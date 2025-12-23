using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Controller;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public class RegisterPatientViewModel : BindableObject
    {
        private readonly NurseController _controller;
        private readonly IAuthService _authService;

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string icNumber = string.Empty;
        public string IcNumber
        {
            get => icNumber;
            set
            {
                icNumber = value;
                OnPropertyChanged();
            }
        }

        private string phNum = string.Empty;
        public string PhoneNumber
        {
            get => phNum;
            set
            {
                phNum = value;
                OnPropertyChanged();
            }
        }

        private bool isEmergency = false;
        public bool IsEmergency
        {
            get => isEmergency;
            set
            {
                isEmergency = value;
                OnPropertyChanged();
            }
        }

        public List<string> ServiceTypes { get; set; } = new()
{
    "General Consultation",
    "Vaccination/Injection",
    "Follow Up Treatment",
    "Test Result Discussion",
    "Medical Checkup",
    "Follow-up",
    "Online"
};

        private string? _selectedServiceType;
        public string? SelectedServiceType
        {
            get => _selectedServiceType;
            set
            {
                _selectedServiceType = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterPatientViewModel(NurseController controller, IAuthService authService)
        {
            _controller = controller;
            _authService = authService;
            RegisterCommand = new Command(OnRegisterClicked);
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async void OnRegisterClicked()
        {
            // Basic field validation
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(IcNumber) ||
                string.IsNullOrWhiteSpace(PhoneNumber) || string.IsNullOrEmpty(SelectedServiceType))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }

            // IC number validation
            var authService = new Services.AuthService(null);
            if (!authService.ValidateICNumber(IcNumber, out string icMessage))
            {
                await Shell.Current.DisplayAlert("Error", icMessage, "OK");
                return;
            }

            // Phone number validation
            if (!authService.ValidatePhoneNumber(PhoneNumber, out string phoneMessage))
            {
                await Shell.Current.DisplayAlert("Error", phoneMessage, "OK");
                return;
            }

            string result = await _controller.RegisterWalkInPatient(
               Name, IcNumber, PhoneNumber, SelectedServiceType, IsEmergency
           );

            if (result == "Success")
            {
                await Shell.Current.DisplayAlert("Success", "Patient registered successfully.", "OK");
                await Shell.Current.GoToAsync("WalkInPatientQueuePage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to register patient.", "OK");
            }
        }
    }
}