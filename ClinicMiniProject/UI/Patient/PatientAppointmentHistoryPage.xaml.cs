using ClinicMiniProject.ViewModels;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Patient;

public partial class PatientAppointmentHistoryPage : ContentPage
{
	public PatientAppointmentHistoryPage(PatientAppointmentHistoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PatientAppointmentHistoryViewModel vm)
        {
            vm.LoadAppointmentsCommand.Execute(null);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///PatientHomePage");
    }
}