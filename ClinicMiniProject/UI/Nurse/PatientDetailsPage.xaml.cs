using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class PatientDetailsPage : ContentPage
{
    private readonly PatientDetailsViewModel _viewModel;

    public PatientDetailsPage(PatientDetailsViewModel viewModel)
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