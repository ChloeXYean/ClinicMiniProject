using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels; 

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentHistoryPage : ContentPage
{
    public AppointmentHistoryPage(string userType = "Doctor")
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var authService = sp?.GetService<IAuthService>();
        var viewModel = sp?.GetService<PatientAppointmentHistoryViewModel>();

        if (viewModel != null)
        {
            viewModel.UserType = userType;
            BindingContext = viewModel;
        }
    }
}