using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.UI.Nurse;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

public partial class InquiryPage : ContentPage
{
    private IAuthService? _authService;
    private bool isDoctor = true; // Default to Doctor

    public InquiryPage()
    {
        InitializeComponent();

        // Try to load ViewModel
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<OnlineMedicalInquiryViewModel>();
        if (vm != null) BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. SAFELY Load Auth Service (If not loaded yet)
        if (_authService == null)
        {
            var sp = this.Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
            _authService = sp?.GetService<IAuthService>();
        }

        // 2. Check Role & Setup Buttons
        CheckUserRole();
        SetupBottomBar();

        // 3. Load Data
        if (BindingContext is OnlineMedicalInquiryViewModel vm)
        {
            vm.LoadAsync();
        }
    }

    private void CheckUserRole()
    {
        if (_authService == null) return;

        var currentUser = _authService.GetCurrentUser();

        // If user is Staff and NOT a doctor, treat as Nurse
        if (currentUser != null && !currentUser.isDoctor)
        {
            isDoctor = false; // It is a Nurse
        }
        else
        {
            isDoctor = true; // It is a Doctor
        }
    }

    private void SetupBottomBar()
    {
        // x:Name="BottomBar" must exist in InquiryPage.xaml
        if (BottomBar != null)
        {
            if (isDoctor)
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}"));
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            }
            else
            {
                // NURSE NAVIGATION
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}"));
                BottomBar.ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///NurseProfile"));
            }

            // Stay on current page (Chat)
            BottomBar.ChatCommand = new Command(() => { });
        }
    }
}