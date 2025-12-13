namespace ClinicMiniProject.UI.Nurse;

public partial class EndConsultationPage : ContentPage
{
    public EndConsultationPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Info", "Navigating to details...", "OK");
    }


}