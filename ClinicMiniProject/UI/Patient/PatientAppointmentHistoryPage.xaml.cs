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
}