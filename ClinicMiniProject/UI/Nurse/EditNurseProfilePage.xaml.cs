using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse;

public partial class EditNurseProfilePage : ContentPage
{
    public EditNurseProfilePage(EditNurseProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}