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
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (WalkInPatientList.SelectedItem != null)
        {
            WalkInPatientList.SelectedItem = null;
        }

        await _viewModel.LoadQueue();
    }

}