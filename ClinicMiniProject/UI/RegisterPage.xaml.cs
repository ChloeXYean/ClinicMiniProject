using ClinicMiniProject.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI
{
    public partial class RegisterPage : ContentPage
    {
        private readonly IAuthService _authService;

        public RegisterPage()
        {
            var sp = Application.Current?.Handler?.MauiContext?.Services;
            _authService = sp?.GetService<IAuthService>();
            InitializeComponent();
        }

        public RegisterPage(IAuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var patient = new Models.Patient
            {
                patient_IC = IcEntry.Text,
                patient_name = FullNameEntry.Text,
                patient_email = EmailEntry.Text,
                patient_contact = PhoneEntry.Text,
                password = PasswordEntry.Text,
                isAppUser = true
            };

            bool success = _authService.RegisterPatient(patient, out string message);

            if (!success)
            {
                await DisplayAlert("Error", message, "OK");
                return;
            }

            // Only check password match after all other validations pass
            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            await DisplayAlert("Success", "Account created", "OK");
            await Shell.Current.GoToAsync("..");
        }

        private void OnShowPasswordClicked(object sender, EventArgs e)
        {
            var passwordEntry = FindByName("PasswordEntry") as Entry;
            var showPasswordBtn = FindByName("ShowPasswordBtn") as ImageButton;
            
            if (passwordEntry != null && showPasswordBtn != null)
            {
                passwordEntry.IsPassword = !passwordEntry.IsPassword;

                if (passwordEntry.IsPassword)
                {
                    showPasswordBtn.Source = "eye_off.png";
                }
                else
                {
                    showPasswordBtn.Source = "eye_on.png";
                }
            }
        }

        private void OnShowConfirmPasswordClicked(object sender, EventArgs e)
        {
            var confirmPasswordEntry = FindByName("ConfirmPasswordEntry") as Entry;
            var showConfirmPasswordBtn = FindByName("ShowConfirmPasswordBtn") as ImageButton;
            
            if (confirmPasswordEntry != null && showConfirmPasswordBtn != null)
            {
                confirmPasswordEntry.IsPassword = !confirmPasswordEntry.IsPassword;

                if (confirmPasswordEntry.IsPassword)
                {
                    showConfirmPasswordBtn.Source = "eye_off.png";
                }
                else
                {
                    showConfirmPasswordBtn.Source = "eye_on.png";
                }
            }
        }

        private async void OnGoToLogin(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
