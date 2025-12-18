using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class DoctorDashboardPage : ContentPage
{
    private readonly DoctorDashboardViewModel _viewModel;

    public DoctorDashboardPage(DoctorDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    private void NavigateToAppointmentScheduleCommand(object sender, EventArgs e)
    {
        _viewModel.NavigateToAppointmentScheduleCommand.Execute(null);
    }

    private void NavigateToAppointmentHistoryCommand(object sender, EventArgs e)
    {
        _viewModel.NavigateToAppointmentHistoryCommand.Execute(null);
    }

    private void NavigateToReportingManagementCommand(object sender, EventArgs e)
    {
        _viewModel.NavigateToReportingManagementCommand.Execute(null);
    }
}