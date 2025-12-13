using ClinicMiniProject.Controller;
namespace ClinicMiniProject.UI.Nurse;


public partial class NurseHomePage : ContentPage
{
    private readonly NurseController _controller;
    public NurseHomePage(NurseController controller)
	{
		InitializeComponent();
		_controller = controller;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardData();
    }
    private async Task LoadDashboardData()
    {
        var upcomingList = await _controller.GetUpcomingAppointment();

        var nextAppointment = upcomingList.FirstOrDefault();

        appDate.Text = $"Date: {DateTime.Now:dd MMMM yyyy}";

        if (nextAppointment != null && nextAppointment.appointedAt.HasValue)
        {
            appTime.Text = $"Time: {nextAppointment.appointedAt.Value:hh:mm tt}";

            // using staff ID for now, should use name
            appDoc.Text = $"Doctor: {nextAppointment.staff_ID}";
        }
        else
        {
            appTime.Text = "Time: --";
            appDoc.Text = "Doctor: --";
        }

        appPendingCount.Text = $"Pending: {upcomingList.Count}";
    }
}