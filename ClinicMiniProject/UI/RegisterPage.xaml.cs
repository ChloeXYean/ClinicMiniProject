using ClinicMiniProject.Models;
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
            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            var patient = new Patient
            {
                patient_IC = IcEntry.Text,
                patient_name = FullNameEntry.Text,
                patient_email = EmailEntry.Text,
                patient_contact = PhoneEntry.Text,
                password = PasswordEntry.Text,
                isAppUser = true
            };

            bool success = _authService.RegisterPatient(patient, out string message);

            if (success)
            {
                await DisplayAlert("Success", "Account created", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }

        private async void OnGoToLogin(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
