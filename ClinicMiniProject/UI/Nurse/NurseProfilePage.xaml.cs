using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse
{
    public partial class NurseProfilePage : ContentPage
    {
        public NurseProfilePage()
        {
            InitializeComponent();
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("..");
        }
    }
}
