using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class AppointmentHistory_NoHistory : ContentPage
{
    public AppointmentHistory_NoHistory()
    {
        InitializeComponent();
        BindingContext = new AppointmentHistoryNoHistoryViewModel();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///PatientHomePage");
    }
}