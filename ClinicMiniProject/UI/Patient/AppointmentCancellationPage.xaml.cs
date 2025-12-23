using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class AppointmentCancellationPage : ContentPage
{
	public AppointmentCancellationPage(AppointmentCancellationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}
}
