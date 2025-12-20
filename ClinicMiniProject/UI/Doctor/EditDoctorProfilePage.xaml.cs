using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor
{
    public partial class EditDoctorProfilePage : ContentPage
    {
        public EditDoctorProfilePage(EditDoctorProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
