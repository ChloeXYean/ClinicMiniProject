using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class EditPatientProfilePage : ContentPage
{
    public EditPatientProfilePage(EditPatientProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}