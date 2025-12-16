using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class NurseHomePage : ContentPage
{
    private readonly NurseHomeViewModel _viewModel;

    public NurseHomePage(NurseHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadDashboardData();
    }
    private void OnViewAppointmentTapped(object sender, EventArgs e)
    {
        _viewModel.ViewAppointmentCommand.Execute(null);
    }

    private void OnAppointmentHistoryTapped(object sender, EventArgs e)
    {
        _viewModel.AppointmentHistoryCommand.Execute(null);
    }

    private void OnReportingManagementTapped(object sender, EventArgs e)
    {
        _viewModel.ReportingManagementCommand.Execute(null);
    }

    private void OnWalkInQueueTapped(object sender, EventArgs e)
    {
        _viewModel.WalkInQueueCommand.Execute(null);
    }

    private void OnRegisterPatientTapped(object sender, EventArgs e)
    {
        _viewModel.RegisterPatientCommand.Execute(null);
    }

    private void OnEndConsultationTapped(object sender, EventArgs e)
    {
        _viewModel.EndConsultationCommand.Execute(null);
    }
}