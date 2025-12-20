using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class ConsultationDetailsPage : ContentPage
{
    public ConsultationDetailsPage(ConsultationDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ConsultationDetailsViewModel vm)
        {
            await vm.LoadQueueAsync(); 
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}");
    }
}