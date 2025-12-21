using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class AppointmentSchedulePage : ContentPage
{
    private readonly AppointmentScheduleViewModel _viewModel;

    public AppointmentSchedulePage(AppointmentScheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("AppointmentSchedulePage.OnAppearing called");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}