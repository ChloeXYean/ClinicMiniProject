using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class EndConsultationPage : ContentPage
{

    public EndConsultationPage(EndConsultationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

}