using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI
{
    public partial class LoginPage : ContentPage
    {
        private readonly IAuthService _authService;

        public LoginPage()
        {
            var sp = Application.Current?.Handler?.MauiContext?.Services;
            _authService = sp?.GetService<IAuthService>() ?? new Services.AuthService();
            _authService.SeedStaff();
            InitializeComponent();
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

                // TODO: role-based routing
                // If user is Staff and isDoctor => navigate to Doctor page
                // If user is Patient => navigate to Patient page
                if (user is Staff)
                    await Shell.Current.GoToAsync("//MainPage");
                else
                    await Shell.Current.GoToAsync("//MainPage");
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
