using ClinicMiniProject.Models;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace ClinicMiniProject.ViewModels
{
    // Add QueryProperty to receive the Patient IC we want to edit
    [QueryProperty(nameof(TargetPatientIC), "TargetPatientIC")]
    public class EditPatientProfileViewModel : BindableObject
    {
        private readonly PatientService _patientService;
        private readonly IAuthService _authService;

        // Property to receive the IC from navigation
        public string TargetPatientIC
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    LoadPatientByIC(value);
                }
            }
        }

        // ... [Keep Name, Contact, Email, IC properties] ...
        private string name;
        public string Name { get => name; set { name = value; OnPropertyChanged(); } }

        private string contact;
        public string Contact { get => contact; set { contact = value; OnPropertyChanged(); } }

        private string email;
        public string Email { get => email; set { email = value; OnPropertyChanged(); } }

        private string ic;
        public string IC { get => ic; set { ic = value; OnPropertyChanged(); } }

        private ImageSource profilePictureSource;
        public ImageSource ProfilePictureSource { get => profilePictureSource; set { profilePictureSource = value; OnPropertyChanged(); } }

        private string newProfilePicturePath;
        public string NewProfilePicturePath { get => newProfilePicturePath; set { newProfilePicturePath = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }

        public EditPatientProfileViewModel(PatientService patientService, IAuthService authService)
        {
            _patientService = patientService;
            _authService = authService;

            SaveCommand = new Command(async () => await OnSave());
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ChangeProfilePictureCommand = new Command(async () => await OnChangeProfilePicture());

            // Default load (for Patient logged in)
            LoadCurrentPatient();
        }

        private void LoadCurrentPatient()
        {
            var p = _authService.GetCurrentPatient();
            if (p != null)
            {
                MapPatientToProperties(p);
            }
        }

        private void LoadPatientByIC(string targetIC)
        {
            var p = _patientService.GetPatientByIC(targetIC);
            if (p != null)
            {
                MapPatientToProperties(p);
            }
        }

        private void MapPatientToProperties(Patient p)
        {
            Name = p.patient_name;
            Contact = p.patient_contact;
            Email = p.patient_email ?? "";
            IC = p.patient_IC;
            ProfilePictureSource = ImageSource.FromFile("profilepicture.png");
        }

        private async Task OnChangeProfilePicture()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select profile photo" });
                if (result != null)
                {
                    NewProfilePicturePath = result.FullPath;
                    ProfilePictureSource = ImageSource.FromFile(result.FullPath);
                    await Shell.Current.DisplayAlert("Success", "Photo selected successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to select photo: {ex.Message}", "OK");
            }
        }

        private async Task OnSave()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Contact))
            {
                await Shell.Current.DisplayAlert("Error", "Name and Contact are required.", "OK");
                return;
            }

            // Update using the IC field (which acts as the ID)
            bool success = await _patientService.UpdatePatientProfileAsync(IC, Name, Contact, Email);

            if (success)
            {
                // Only update Session if the logged-in user is the one being edited
                var currentPatient = _authService.GetCurrentPatient();
                if (currentPatient != null && currentPatient.patient_IC == IC)
                {
                    currentPatient.patient_name = Name;
                    currentPatient.patient_contact = Contact;
                    currentPatient.patient_email = Email;
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