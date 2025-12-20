namespace ClinicMiniProject.UI.Patient;

public partial class InquiryHistory : ContentPage
{
	public InquiryHistory()
	{
		InitializeComponent();

		var sp = Application.Current?.Handler?.MauiContext?.Services;
		var viewModel = sp?.GetService<ClinicMiniProject.ViewModels.OnlineMedicalInquiryViewModel>();
		BindingContext = viewModel;
	}
}