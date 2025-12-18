using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class WalkInPatientQueuePage : ContentPage
{
    public WalkInPatientQueuePage(WalkInPatientQueueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

}