using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI.Doctor;
using ClinicMiniProject.UI.Nurse;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI
{
    public partial class LoginPage : ContentPage
    {

        private readonly IAuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            var sp = Application.Current?.Handler?.MauiContext?.Services;
            _authService = sp?.GetService<IAuthService>();
            if (_authService == null)
            {
                DisplayAlert("FATAL ERROR", "AuthService could not be resolved", "OK");
                return;
            }
        }

        public LoginPage(IAuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var user = _authService.Login(IcEntry.Text, PasswordEntry.Text, out string message);

            if (user != null)
            {
                await DisplayAlert("Success", "Login successful", "OK");

                if (user is Models.Patient)
                {
                    await Shell.Current.GoToAsync("///PatientHomePage");
                }
                else if (user is Staff staff)
                {
                    if (staff.isDoctor)
                    {
                        // Go to Doctor Dashboard
                        await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}");
                    }
                    else
                    {
                        // Go to Nurse Home Page
                        await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}");
                    }
                }
            }
            else
                await DisplayAlert("Error", message, "OK");
        }

        private async void OnGoToRegister(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}
