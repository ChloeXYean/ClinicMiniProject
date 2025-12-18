using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class SelectDoctorPage : ContentPage
{
	public SelectDoctorPage()
	{
		InitializeComponent();
        BindingContext = new SelectDoctorViewModel(); // Ensure ViewModel is wired up
	}
}
