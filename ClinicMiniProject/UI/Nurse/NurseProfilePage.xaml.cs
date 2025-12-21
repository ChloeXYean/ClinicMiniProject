using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Nurse
{
    public partial class NurseProfilePage : ContentPage
    {

        public NurseProfilePage(NurseProfileViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is NurseProfileViewModel vm)
            {
                await vm.RefreshAsync();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}