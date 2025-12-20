using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels; 

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentHistoryPage : ContentPage
{
    private readonly string _userType;

    public AppointmentHistoryPage(string userType = "Doctor")
    {
        InitializeComponent();
        _userType = userType;

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var authService = sp?.GetService<IAuthService>();
        var viewModel = sp?.GetService<PatientAppointmentHistoryViewModel>();

        if (viewModel != null)
        {
            viewModel.UserType = _userType;
            BindingContext = viewModel;
        }

        if (BottomBar != null)
        {
            if (userType == "Nurse")
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseHomePage"));
            }
            else
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
            }
            BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
            BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///Profile"));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_userType == "Nurse")
        {
            await Shell.Current.GoToAsync("///NurseHomePage");
        }
        else
        {
            await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}");
        }
    }
}