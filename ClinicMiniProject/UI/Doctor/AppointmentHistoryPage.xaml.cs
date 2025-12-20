using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels; 

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentHistoryPage : ContentPage, IQueryAttributable
{
    private string _userType = "Doctor";

    public AppointmentHistoryPage()
    {
        InitializeComponent();
        // Initial setup
        SetupCommands();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("UserType"))
        {
            _userType = query["UserType"].ToString();

            // Update the ViewModel if it exists
            if (BindingContext is PatientAppointmentHistoryViewModel vm)
            {
                vm.UserType = _userType;
                // Optionally reload data if the ViewModel requires it
                vm.LoadAppointmentsCommand.Execute(null);
            }

            // Re-setup the BottomBar commands with the correct UserType
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
            }
            else
            {
                // Navigate back to Doctor Dashboard
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
            }

            BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
            BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));

            // Note: If user is Nurse, Profile command might need to point to NurseProfile, 
            // but usually this page is shared. You might want to conditionalize Profile too:
            if (_userType == "Nurse")
            {
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseProfile"));
            }
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