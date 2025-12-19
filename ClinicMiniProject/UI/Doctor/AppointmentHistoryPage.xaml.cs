using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;

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

    public AppointmentHistoryPage(string userType = "Doctor")
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var auth = sp?.GetService<Services.Interfaces.IAuthService>();
        var name = auth?.GetCurrentUser()?.staff_name ?? string.Empty;
        TopBar.UserName = name;

        if (userType == "Nurse")
        {
            BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseHomePage"));
        }
        else
        {
            BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(DoctorDashboardPage)));
        }
        BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
        BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("Profile"));
    }
}
