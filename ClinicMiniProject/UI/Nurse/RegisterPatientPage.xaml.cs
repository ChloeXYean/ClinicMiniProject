using ClinicMiniProject.Controller;
using ClinicMiniProject.Services;

namespace ClinicMiniProject.UI.Nurse;

public partial class RegisterPatientPage : ContentPage
{
    private readonly NurseController _controller;
    private readonly IStaffService _staffService;
    public RegisterPatientPage(NurseController controller, IStaffService staffService)
	{
		InitializeComponent();
        _controller = controller;
        _staffService = staffService;
        PopulateServiceTypes();
        LoadDoctors();

    }
    private void LoadDoctors()
    {
        var doctors = _staffService.GetAllDocs();
        DoctorPicker.ItemsSource = doctors;
        DoctorPicker.ItemDisplayBinding = new Binding("staff_name");
    }

    private void PopulateServiceTypes()
    {
        var serviceTypes = new List<string>
        {
            "General Consultation",
            "Follow up treatment",
            "Test Result Discussion",
            "Vaccination/Injection",
            "Blood test",
            "Blood pressure test",
            "Sugar test"
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
            string.IsNullOrWhiteSpace(PhoneEntry.Text) ||
            ServiceTypePicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        string? selectedDoctorId = null;

        if (DoctorPicker.SelectedItem is Staff selectedDoctor)
        {
            selectedDoctorId = selectedDoctor.staff_ID;
        }

        bool success = await _controller.RegisterWalkInPatient(
            NameEntry.Text,
            IcEntry.Text,
            PhoneEntry.Text,
            //ServiceTypePicker.SelectedItem.ToString()!,
            selectedDoctorId
        );

        if (success)
        {
            await DisplayAlert("Success", "Patient registered successfully.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to register patient.", "OK");
        }
    }
}
