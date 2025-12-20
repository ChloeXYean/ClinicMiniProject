using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Patient;

public partial class OnlineInquiryPatient : ContentPage
{
	private readonly OnlineMedicalInquiryViewModel? _viewModel;

	public OnlineInquiryPatient()
	{
		InitializeComponent();
		
		// Get ViewModel from dependency injection
		var sp = Application.Current?.Handler?.MauiContext?.Services;
		_viewModel = sp?.GetService<OnlineMedicalInquiryViewModel>();
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// Initialize doctor list for inquiry form
		if (_viewModel != null)
		{
			try
			{
				await _viewModel.InitializePatientDataAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to load doctors: {ex.Message}", "OK");
			}
		}
	}
}