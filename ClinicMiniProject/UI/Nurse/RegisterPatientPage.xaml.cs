namespace ClinicMiniProject.UI.Nurse;

public partial class RegisterPatientPage : ContentPage
{
	public RegisterPatientPage()
	{
		InitializeComponent();
        PopulateServiceTypes();
    }
    private void PopulateServiceTypes()
    {
        var serviceTypes = new List<string>
        {
            "General Consultation",
            "Specialist Consultation",
            "Health Screening",
            "Vaccination",
            "Follow-up Visit"
        };
        ServiceTypePicker.ItemsSource = serviceTypes;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(IcEntry.Text) ||
            string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (ServiceTypePicker.SelectedIndex == -1)
        {
            await DisplayAlert("Error", "Please select a service type.", "OK");
            return;
        }

        // Success Action --> save to database
        // For now, just show a success message and go back.
        await DisplayAlert("Success", $"Patient {NameEntry.Text} has been registered for {ServiceTypePicker.SelectedItem}.", "OK");

        await Navigation.PopAsync();
    }
}