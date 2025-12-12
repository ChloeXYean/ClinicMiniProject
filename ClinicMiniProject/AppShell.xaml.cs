using ClinicMiniProject.UI;

namespace ClinicMiniProject
{
    public partial class AppShell : Shell
    {
        bool _navigatedToLogin;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_navigatedToLogin) return;
            _navigatedToLogin = true;
            // Absolute or relative route depending on your shell hierarchy
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}
