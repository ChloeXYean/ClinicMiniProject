using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class InquiryPage : ContentPage
{
    public InquiryPage()
    {
        InitializeComponent();

		var sp = Application.Current?.Handler?.MauiContext?.Services;
		var vm = sp?.GetService<OnlineMedicalInquiryViewModel>();
		if (vm != null)
			BindingContext = vm;
    }
}
