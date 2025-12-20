using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.UI.Nurse;
using ClinicMiniProject.Services.Interfaces; 

namespace ClinicMiniProject.UI.Doctor;

public partial class ConsultationDetailsPage : ContentPage
{
    public ConsultationDetailsPage()
    {
        InitializeComponent();
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<ConsultationDetailsViewModel>();
        if (vm != null)
            BindingContext = vm;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var authService = sp?.GetService<IAuthService>();

        var user = authService?.GetCurrentUser();

        if (user != null && !user.isDoctor)
        {
            await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}");
        }
        else
        {
            await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}");
        }
    }
}