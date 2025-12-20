using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

// Implement IQueryAttributable to support passing data via Shell (e.g. ?UserType=Nurse)
public partial class AppointmentHistoryPage : ContentPage, IQueryAttributable
{
    private string _userType = "Doctor"; // Default to Doctor

    public AppointmentHistoryPage()
    {
        InitializeComponent();

        // --- FROM SNIPPET 1: Resolve ViewModel Manually ---
        // This ensures the ViewModel is loaded even if the Constructor injection fails.
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var viewModel = sp?.GetService<PatientAppointmentHistoryViewModel>();

        if (viewModel != null)
        {
            viewModel.UserType = _userType;
            BindingContext = viewModel;
        }

        // --- FROM SNIPPET 2: Setup UI Commands ---
        SetupCommands();
    }

    // Called automatically when navigating with parameters (e.g. Shell.GoToAsync("...?UserType=Nurse"))
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("UserType"))
        {
            _userType = query["UserType"].ToString();

            // Update the ViewModel if it exists
            if (BindingContext is PatientAppointmentHistoryViewModel vm)
            {
                vm.UserType = _userType;
                // Reload data based on the new user type
                if (vm.LoadAppointmentsCommand.CanExecute(null))
                {
                    vm.LoadAppointmentsCommand.Execute(null);
                }
            }

            // Re-configure the buttons (Home/Profile) based on the new UserType
            SetupCommands();
        }
    }

    private void SetupCommands()
    {
        if (BottomBar != null)
        {
            if (_userType == "Nurse")
            {
                // Navigate back to Nurse Home
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseHomePage"));
                // Nurse Profile
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseProfile"));
            }
            else
            {
                // Navigate back to Doctor Dashboard
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
                // Doctor Profile
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            }

            // Chat command is shared
            BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
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