using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Patient;

public partial class SelectDoctorPage : ContentPage
{
	public SelectDoctorPage(SelectDoctorViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
