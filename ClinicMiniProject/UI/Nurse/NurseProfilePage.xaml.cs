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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is NurseProfileViewModel vm)
            {
                vm.RefreshCommand.Execute(null);
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}