using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class ActiveConsultationPage : ContentPage
{
    public ActiveConsultationPage(ActiveConsultationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
