using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels;
using System.Windows.Input;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentHistoryPage : ContentPage, IQueryAttributable
{
    private string _userType = "Doctor";

    public AppointmentHistoryPage()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var viewModel = sp?.GetService<PatientAppointmentHistoryViewModel>();

        if (viewModel != null)
        {
            viewModel.UserType = _userType;
            BindingContext = viewModel;
        }

        SetupCommands();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("UserType"))
        {
            _userType = query["UserType"].ToString();

            if (BindingContext is PatientAppointmentHistoryViewModel vm)
            {
                vm.UserType = _userType;
                if (vm.LoadAppointmentsCommand.CanExecute(null))
                {
                    vm.LoadAppointmentsCommand.Execute(null);
                }
            }
            SetupCommands();
        }
    }

    private void SetupCommands()
    {
        if (BottomBar != null)
        {
            if (_userType == "Nurse")
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseHomePage"));
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseProfile"));
            }
            else
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            }

            BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
        }
    }
}