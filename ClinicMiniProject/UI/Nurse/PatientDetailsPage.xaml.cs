namespace ClinicMiniProject.UI.Nurse;

public partial class PatientDetailsPage : ContentPage
{
	public PatientDetailsPage()
	{
		InitializeComponent();
	}

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Action", "Opening Edit Form...", "OK");
    }
}