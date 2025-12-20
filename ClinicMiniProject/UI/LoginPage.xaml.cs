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
            var icEntry = FindByName("IcEntry") as Entry;
            var passwordEntry = FindByName("PasswordEntry") as Entry;
            
            try
            {
                var user = await _authService.LoginAsync(icEntry?.Text ?? "", passwordEntry?.Text ?? "");

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
            }
            
            catch (InvalidOperationException ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "An error occurred during login. Please try again.", "OK");
            }
            
        }

        private void OnShowPasswordClicked(object sender, EventArgs e)
        {
            // Toggle the IsPassword property
            var passwordEntry = FindByName("PasswordEntry") as Entry;
            var showPasswordBtn = FindByName("ShowPasswordBtn") as ImageButton;
            
            if (passwordEntry != null && showPasswordBtn != null)
            {
                passwordEntry.IsPassword = !passwordEntry.IsPassword;

                // Change the icon based on the state
                if (passwordEntry.IsPassword)
                {
                    showPasswordBtn.Source = "eye_off.png"; // Icon for hidden state
                }
                else
                {
                    showPasswordBtn.Source = "eye_on.png";  // Icon for visible state
                }
            }
        }

        private async void OnGoToRegister(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}
