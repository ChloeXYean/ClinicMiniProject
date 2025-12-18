using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.UI.Doctor;

public partial class ReportingManagementPage : ContentPage
{

    public ReportingManagementPage(string userType = "Doctor")
    {
        InitializeComponent();

		var sp = Application.Current?.Handler?.MauiContext?.Services;
		var vm = sp?.GetService<ReportingManagementViewModel>();
		if (vm != null)
		{
			vm.UserType = userType;
			BindingContext = vm;
		}

        // Set up BackNavBar
        BackBar.Title = "Reporting Management";
        BackBar.BackCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnDayChecked(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && BindingContext is ReportingManagementViewModel vm)
            vm.ReportPeriodType = "Day";
    }

    private void OnWeekChecked(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && BindingContext is ReportingManagementViewModel vm)
            vm.ReportPeriodType = "Week";
    }

    private void OnMonthChecked(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && BindingContext is ReportingManagementViewModel vm)
            vm.ReportPeriodType = "Month";
    }
}
