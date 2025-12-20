using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class PatientHomePage : ContentPage
{
    private readonly PatientHomeViewModel _viewModel;

    public PatientHomePage(PatientHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ReloadData();
    }
}