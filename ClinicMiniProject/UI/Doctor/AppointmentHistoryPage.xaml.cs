using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentHistoryPage : ContentPage
{
    public AppointmentHistoryPage()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var auth = sp?.GetService<Services.Interfaces.IAuthService>();
        var name = auth?.GetCurrentUser()?.staff_name ?? string.Empty;
        TopBar.UserName = name;

        BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(DoctorDashboardPage)));
        BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
        BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("Profile"));
    }
}
