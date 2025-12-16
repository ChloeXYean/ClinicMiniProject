using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class WalkInPatientQueuePage : ContentPage
{
    private readonly WalkInPatientQueueViewModel _viewModel;

    public WalkInPatientQueuePage(WalkInPatientQueueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        _viewModel.BackCommand.Execute(null);
    }

    private void OnClickViewDetails(object sender, EventArgs e)
    {
        _viewModel.ViewDetailsCommand.Execute(null);
    }
}