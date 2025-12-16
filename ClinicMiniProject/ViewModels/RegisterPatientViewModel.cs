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

        private string name;
        public string Name { 
            get => name; 
            set {
                name = value; 
                OnPropertyChanged(); 
            } 
        }

        private string icNumber;
        public string IcNumber { 
            get => icNumber; 
            set {
                icNumber = value; 
                OnPropertyChanged(); 
            } 
        }

        private string phNum;
        public string PhoneNumber { 
            get => phNum; 
            set {
                phNum = value; 
                OnPropertyChanged(); 
            } 
        }

        // Dropdown Data Sources
        public ObservableCollection<Staff> Doctors { get; set; } = new();
        public List<string> ServiceTypes { get; set; } = new()
        {
            "General Consultation", "Follow up treatment", "Test Result Discussion",
            "Vaccination/Injection", "Blood test", "Blood pressure test", "Sugar test"
        };

        // Selected Items
        private Staff selectedDoctor;
        public Staff SelectedDoctor { 
            get => selectedDoctor; 
            set {
                selectedDoctor = value; 
                OnPropertyChanged(); 
            } 
        }

        private string _selectedServiceType;
        public string SelectedServiceType {
            get => _selectedServiceType; 
            set { 
                _selectedServiceType = value; 
                OnPropertyChanged(); 
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterPatientViewModel(NurseController controller, IStaffService staffService)
        {
            _controller = controller;
            _staffService = staffService;

            RegisterCommand = new Command(OnRegisterClicked);
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            LoadDoctors();
        }

        private void LoadDoctors()
        {
            var doctorList = _staffService.GetAllDocs();
            Doctors.Clear();
            foreach (var doc in doctorList)
            {
                Doctors.Add(doc);
            }
        }

        private async void OnRegisterClicked()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(IcNumber) ||
                string.IsNullOrWhiteSpace(PhoneNumber) || SelectedServiceType == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }

            bool success = await _controller.RegisterWalkInPatient(
                Name, IcNumber, PhoneNumber, SelectedDoctor?.staff_ID
            );

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Patient registered successfully.", "OK");
                await Shell.Current.GoToAsync(".."); // Go back
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to register patient.", "OK");
            }
        }
    }
}