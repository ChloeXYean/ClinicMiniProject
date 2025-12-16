using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class EndConsultationPage : ContentPage
{
    private readonly EndConsultationViewModel _viewModel;

    public EndConsultationPage(EndConsultationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        _viewModel.BackCommand.Execute(null);
    }

    private void OnDetailsClicked(object sender, EventArgs e)
    {
        _viewModel.ViewDetailsCommand.Execute(null);
    }
}