using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class WalkInPatientQueuePage : ContentPage
{
    private readonly WalkInPatientQueueViewModel _viewModel;
    public WalkInPatientQueuePage(WalkInPatientQueueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Trigger the data load
        _viewModel.LoadQueue();
    }

}