using ClinicMiniProject.Services;

namespace ClinicMiniProject.UI
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService = new AuthService();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var user = _authService.Login(IcEntry.Text, PasswordEntry.Text, out string message);

            if (user != null)
                await DisplayAlert("Success", "Login successful", "OK");
            else
                await DisplayAlert("Error", message, "OK");
        }

        private async void OnGoToRegister(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
