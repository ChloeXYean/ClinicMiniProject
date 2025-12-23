using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class PatientConsultationDetailsPage : ContentPage
{
	public PatientConsultationDetailsPage(PatientConsultationDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}