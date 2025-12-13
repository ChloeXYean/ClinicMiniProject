using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentSchedulePage : ContentPage
{
    public AppointmentSchedulePage()
    {
        InitializeComponent();

		var sp = Application.Current?.Handler?.MauiContext?.Services;
		var vm = sp?.GetService<AppointmentScheduleViewModel>();
		if (vm != null)
			BindingContext = vm;

		BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(DoctorDashboardPage)));
		BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
		BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("Profile"));
    }

	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
}
