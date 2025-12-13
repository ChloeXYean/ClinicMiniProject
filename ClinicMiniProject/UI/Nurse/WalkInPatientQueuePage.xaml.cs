namespace ClinicMiniProject.UI.Nurse;

public partial class WalkInPatientQueuePage : ContentPage
{
    public WalkInPatientQueuePage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnClickViewDetails(object sender, EventArgs e)
    {
        await DisplayAlert("Info", "Navigating to details...", "OK");
    }

}