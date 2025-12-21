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

        // --- Fields ---
        private string name = string.Empty;
        private string icNumber = string.Empty;
        private string phNum = string.Empty;
        private bool isEmergency = false;
        private string? _selectedServiceType;

        // --- Error Message Properties (For High-Class UI) ---
        private string _nameErrorText = string.Empty;
        private bool _isNameInvalid = false;

        private string _icErrorText = string.Empty;
        private bool _isIcInvalid = false;

        private string _phoneErrorText = string.Empty;
        private bool _isPhoneInvalid = false;

        // --- Public Properties ---
        public string Name { get => name; set { name = value; OnPropertyChanged(); ClearErrors(); } }
        public string IcNumber { get => icNumber; set { icNumber = value; OnPropertyChanged(); ClearErrors(); } }
        public string PhoneNumber { get => phNum; set { phNum = value; OnPropertyChanged(); ClearErrors(); } }
        public bool IsEmergency { get => isEmergency; set { isEmergency = value; OnPropertyChanged(); } }
        public string? SelectedServiceType { get => _selectedServiceType; set { _selectedServiceType = value; OnPropertyChanged(); } }

        // --- Error Bindings ---
        public string NameErrorText { get => _nameErrorText; set { _nameErrorText = value; OnPropertyChanged(); } }
        public bool IsNameInvalid { get => _isNameInvalid; set { _isNameInvalid = value; OnPropertyChanged(); } }

        public string IcErrorText { get => _icErrorText; set { _icErrorText = value; OnPropertyChanged(); } }
        public bool IsIcInvalid { get => _isIcInvalid; set { _isIcInvalid = value; OnPropertyChanged(); } }

        public string PhoneErrorText { get => _phoneErrorText; set { _phoneErrorText = value; OnPropertyChanged(); } }
        public bool IsPhoneInvalid { get => _isPhoneInvalid; set { _isPhoneInvalid = value; OnPropertyChanged(); } }

        public List<string> ServiceTypes { get; set; } = new()
        {
            "General Consultation", "Vaccination/Injection", "Follow Up Treatment",
            "Test Result Discussion", "Medical Checkup", "Follow-up", "Online"
        };

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterPatientViewModel(NurseController controller, IAuthService authService)
        {
            _controller = controller;
            _authService = authService;
            RegisterCommand = new Command(OnRegisterClicked);
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private void ClearErrors()
        {
            IsNameInvalid = false;
            IsIcInvalid = false;
            IsPhoneInvalid = false;
        }

        private async void OnRegisterClicked()
        {
            bool isValid = true;
            string errorMessage = string.Empty;

            // 1. Validate Name
            if (string.IsNullOrWhiteSpace(Name))
            {
                NameErrorText = "Full name is required.";
                IsNameInvalid = true;
                isValid = false;
            }

            // 2. Validate IC (Using AuthService Logic)
            if (!_authService.ValidateICNumber(IcNumber, out string icMsg))
            {
                IcErrorText = icMsg;
                IsIcInvalid = true;
                isValid = false;
            }

            // 3. Validate Phone (Using AuthService Logic)
            if (!_authService.ValidatePhoneNumber(PhoneNumber, out string phoneMsg))
            {
                PhoneErrorText = phoneMsg;
                IsPhoneInvalid = true;
                isValid = false;
            }

            // 4. Validate Service Type
            if (string.IsNullOrEmpty(SelectedServiceType))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please select a service type.", "OK");
                return;
            }

            if (!isValid) return; // Stop if any validation failed

            // 5. Proceed with Registration
            string result = await _controller.RegisterWalkInPatient(
               Name, IcNumber, PhoneNumber, SelectedServiceType, IsEmergency
            );

            if (result == "Success")
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Patient registered successfully.", "OK");
                await Shell.Current.GoToAsync("WalkInPatientQueuePage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to register patient.", "OK");
            }
        }
    }
}