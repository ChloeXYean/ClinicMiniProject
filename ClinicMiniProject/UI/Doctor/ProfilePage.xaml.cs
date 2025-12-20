using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class ProfilePage : ContentPage
{
    private readonly DoctorProfileViewModel _vm;

    public ProfilePage(DoctorProfileViewModel viewModel)
    {
        InitializeComponent();
        
        _vm = viewModel;
        BindingContext = _vm;

        BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
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
        await _vm.RefreshAsync();
        System.Diagnostics.Debug.WriteLine($"Profile loaded - DoctorId: {_vm.DoctorId}, Name: {_vm.DoctorName}, Phone: {_vm.PhoneNumber}");
    }
}
