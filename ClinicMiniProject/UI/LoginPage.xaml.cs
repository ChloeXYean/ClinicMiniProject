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
            if (_authService != null)
            {
                DisplayAlert("FATAL ERROR", "AuthService could not be resolved", "OK");
                return;
            }
            try
            {
                _authService.SeedStaff();
            }
            catch (Exception ex)
            {
                //Database error handling
                DisplayAlert("Database error", "Could not connect to database", "Check IP / Server status" + ex.Message, "OK");
            }
        }

        public LoginPage(IAuthService authService)
        {
            _authService = authService;
            _authService.SeedStaff();
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var user = _authService.Login(IcEntry.Text, PasswordEntry.Text, out string message);

            if (user != null)
            {
                await DisplayAlert("Success", "Login successful", "OK");

                if (user is Staff staff)
                {
                    if (staff.isDoctor)
                    {
                        // Go to Doctor Dashboard
                        await Shell.Current.GoToAsync(nameof(DoctorDashboardPage));
                    }
                    else
                    {
                        // Go to Nurse Home Page
                        await Shell.Current.GoToAsync(nameof(NurseHomePage));
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
