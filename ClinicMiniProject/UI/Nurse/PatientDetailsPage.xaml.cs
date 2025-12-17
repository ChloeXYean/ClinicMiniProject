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

    private void OnBackClicked(object sender, EventArgs e)
    {
        _viewModel.BackCommand.Execute(null);
    }

    private void OnUpdateClicked(object sender, EventArgs e)
    {
        _viewModel.UpdateCommand.Execute(null);
    }
}