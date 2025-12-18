using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class NurseHomePage : ContentPage
{
    public NurseHomePage(NurseHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NurseHomeViewModel vm)
        {
            vm.LoadDashboardData();
        }
    }
}