using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels;
using System.Windows.Input;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentHistoryPage : ContentPage, IQueryAttributable
{
    private string _userType = "Doctor";

    // Command bound to the BackNavBar in XAML
    public ICommand BackCommand { get; private set; }

    public AppointmentHistoryPage()
    {
        InitializeComponent();

        // Initialize the Back Command
        BackCommand = new Command(async () => await OnBack());

        // Manual ViewModel Resolution
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
        // Now BottomBar exists in XAML, so this will work
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

    private async Task OnBack()
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