using ClinicMiniProject.ViewModels;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentSchedulePage : ContentPage
{
    public AppointmentSchedulePage(AppointmentScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public AppointmentSchedulePage(IAuthService authService, IAppointmentScheduleService scheduleService, IStaffService staffService = null, string userType = "Doctor")
    {
        InitializeComponent();
        var viewModel = new AppointmentScheduleViewModel(authService, scheduleService, staffService, userType);
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}