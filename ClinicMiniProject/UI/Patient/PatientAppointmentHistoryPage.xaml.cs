using ClinicMiniProject.ViewModels;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Patient;

public partial class PatientAppointmentHistoryPage : ContentPage
{
	public PatientAppointmentHistoryPage(PatientAppointmentHistoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

    private readonly PatientAppointmentHistoryViewModel _viewModel;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadAppointmentsCommand.Execute(null);
    }
}