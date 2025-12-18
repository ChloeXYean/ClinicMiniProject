using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class InquiryHistoryPage : ContentPage
{
    private readonly DoctorDashboardViewModel _viewModel;

    public InquiryHistoryPage(DoctorDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
