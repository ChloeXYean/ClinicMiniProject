using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class ProfilePage : ContentPage
{
    private DoctorProfileViewModel? _vm;

    public ProfilePage()
    {
        InitializeComponent();

		var sp = Application.Current?.Handler?.MauiContext?.Services;
		_vm = sp?.GetService<DoctorProfileViewModel>();
		if (_vm != null)
			BindingContext = _vm;

		BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(DoctorDashboardPage)));
		BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("Inquiry"));
		BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("Profile"));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm != null)
            await _vm.RefreshAsync();
    }
}
