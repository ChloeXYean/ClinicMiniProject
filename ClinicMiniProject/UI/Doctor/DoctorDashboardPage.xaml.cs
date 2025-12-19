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
}