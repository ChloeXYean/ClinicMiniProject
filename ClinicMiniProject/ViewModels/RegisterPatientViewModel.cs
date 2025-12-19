using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Controller;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services;

namespace ClinicMiniProject.ViewModels
{
    public class RegisterPatientViewModel : BindableObject
    {
        private readonly NurseController _controller;
        private readonly IStaffService _staffService;

        private string name = string.Empty;
        public string Name { 
            get => name; 
            set {
                name = value; 
                OnPropertyChanged(); 
            } 
        }

        private string icNumber = string.Empty;
        public string IcNumber { 
            get => icNumber; 
            set {
                icNumber = value; 
                OnPropertyChanged(); 
            } 
        }

        private string phNum = string.Empty;
        public string PhoneNumber { 
            get => phNum; 
            set {
                phNum = value; 
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
        public string? SelectedServiceType {
            get => _selectedServiceType; 
            set { 
                _selectedServiceType = value; 
                OnPropertyChanged(); 
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterPatientViewModel(NurseController controller)
        {
            _controller = controller;
            RegisterCommand = new Command(OnRegisterClicked);
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async void OnRegisterClicked()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(IcNumber) ||
                string.IsNullOrWhiteSpace(PhoneNumber) || string.IsNullOrEmpty(SelectedServiceType))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }

            string result = await _controller.RegisterWalkInPatient(
               Name, IcNumber, PhoneNumber, SelectedServiceType
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