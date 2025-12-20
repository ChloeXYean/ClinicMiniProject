using ClinicMiniProject.Models;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class EditPatientProfileViewModel : BindableObject
    {
        private readonly PatientService _patientService;
        private readonly IAuthService _authService;

        private string name;
        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        private string contact;
        public string Contact
        {
            get => contact;
            set { contact = value; OnPropertyChanged(); }
        }

        private string email;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        private string ic;
        public string IC
        {
            get => ic;
            set { ic = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditPatientProfileViewModel(PatientService patientService, IAuthService authService)
        {
            _patientService = patientService;
            _authService = authService;

            SaveCommand = new Command(async () => await OnSave());
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            LoadCurrentPatient();
        }

        private void LoadCurrentPatient()
        {
            var p = _authService.GetCurrentPatient();
            if (p != null)
            {
                Name = p.patient_name;
                Contact = p.patient_contact;
                Email = p.patient_email ?? "";
                IC = p.patient_IC;
            }
        }

        private async Task OnSave()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Contact))
            {
                await Shell.Current.DisplayAlert("Error", "Name and Contact are required.", "OK");
                return;
            }

            bool success = await _patientService.UpdatePatientProfileAsync(IC, Name, Contact, Email);

            if (success)
            {
                // Refresh the current session data if necessary
                var p = _authService.GetCurrentPatient();
                if (p != null)
                {
                    p.patient_name = Name;
                    p.patient_contact = Contact;
                    p.patient_email = Email;
                }

                await Shell.Current.DisplayAlert("Success", "Profile updated successfully!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update profile.", "OK");
            }
        }
    }
}