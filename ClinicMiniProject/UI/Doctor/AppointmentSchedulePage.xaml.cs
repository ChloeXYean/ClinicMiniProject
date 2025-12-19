using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentSchedulePage : ContentPage
{
    public AppointmentSchedulePage(AppointmentScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // You can keep this, or use the Command in the ViewModel (better)
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}