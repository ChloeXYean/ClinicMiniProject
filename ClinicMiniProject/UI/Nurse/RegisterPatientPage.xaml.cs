using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class RegisterPatientPage : ContentPage
{
    private readonly RegisterPatientViewModel _viewModel;

    public RegisterPatientPage(RegisterPatientViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        _viewModel.BackCommand.Execute(null);
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        _viewModel.RegisterCommand.Execute(null);
    }

}