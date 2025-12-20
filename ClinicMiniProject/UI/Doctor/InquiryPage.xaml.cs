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

		var auth = sp?.GetService<Services.Interfaces.IAuthService>();
		var name = auth?.GetCurrentUser()?.staff_name ?? string.Empty;
		TopBar.UserName = name;

		BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
		BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
		BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
    }
}
