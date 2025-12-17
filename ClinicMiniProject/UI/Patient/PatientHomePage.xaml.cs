using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class PatientHomePage : ContentPage
{
    public PatientHomePage(PatientHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}