using System.Windows.Input;

namespace ClinicMiniProject.UI.Patient;

public partial class PatientHomePage : ContentPage
{
	public PatientHomePage()
	{
		InitializeComponent();
	}
/*
    public ICommand HomeCommand { get; }
    public ICommand InquiryHistoryCommand { get; }
    public ICommand ProfileCommand { get; }

    public PatientHomePage()
    {
        InitializeComponent();

        // Setup Navigation Commands for Bottom Bar
        HomeCommand = new Command(() => { *//* Already on Home *//* });

        // Chat -> Inquiry History (for Patient)
        InquiryHistoryCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(InquiryHistoryPage)));

        // Profile -> Patient Details (as requested)
        ProfileCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(PatientDetailsPage)));

        BindingContext = this;
    }

    private async void OnAppointmentHistoryTapped(object sender, EventArgs e)
    {
        // Logic to decide between NoHistory or History could go here
        // For now, we route to the main history page which handles the empty state
        await Shell.Current.GoToAsync(nameof(AppointmentHistoryPage));
    }

    private async void OnOnlineInquiryTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OnlineMedicalInquiryPage));
    }*/
}