using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI.Nurse;
using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

public partial class InquiryHistoryPage : ContentPage, IQueryAttributable
{
    private string _userType = "Doctor";
    private IAuthService? _authService;

    public InquiryHistoryPage()
    {
        InitializeComponent();

        // Load ViewModel
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<DoctorDashboardViewModel>();
        if (vm != null) BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. SAFELY Load Auth Service
        if (_authService == null)
        {
            var sp = this.Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
            _authService = sp?.GetService<IAuthService>();
        }

        SetupNavigation();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("UserType"))
        {
            _userType = query["UserType"].ToString();
            SetupNavigation();
        }
    }

    private void SetupNavigation()
    {
        // Check if Nurse based on UserType OR Auth Service
        bool isNurse = _userType == "Nurse";

        if (_authService != null)
        {
            var currentUser = _authService.GetCurrentUser();
            if (currentUser != null && !currentUser.isDoctor) isNurse = true;
        }

        // Setup Buttons (Requires x:Name="BottomBar" in XAML)
        if (BottomBar != null)
        {
            if (isNurse)
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}"));
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseProfile"));
            }
            else
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            }
            // Chat button goes to Main Inquiry Page
            BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));
        }
    }

    // Fix for "EventHandler not found" crash
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is DoctorDashboardViewModel vm)
        {
            vm.FilterInquiries(e.NewTextValue);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        bool isNurse = _userType == "Nurse";
        if (_authService?.GetCurrentUser()?.isDoctor == false) isNurse = true;

        if (isNurse) await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}");
        else await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}");
    }
}