using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.UI.Doctor; // Needed for Inquiry reference if shared
// using ClinicMiniProject.UI.Nurse; // Current namespace

namespace ClinicMiniProject.UI.Nurse
{
    public partial class NurseProfilePage : ContentPage
    {
        public NurseProfilePage()
        {
            InitializeComponent();

            var sp = Application.Current?.Handler?.MauiContext?.Services;
            var vm = sp?.GetService<NurseProfileViewModel>();

            if (vm != null)
            {
                BindingContext = vm;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is NurseProfileViewModel vm)
            {
                
                await vm.RefreshAsync();
            }

            SetupBottomBar();
        }

        private void SetupBottomBar()
        {
            if (BottomBar != null)
            {
                BottomBar.HomeCommand = new Command(async () => await Shell.Current.GoToAsync($"///{nameof(NurseHomePage)}"));

                BottomBar.ChatCommand = new Command(async () => await Shell.Current.GoToAsync("///Inquiry"));

                BottomBar.ProfileCommand = new Command(async () =>
                {
                    if (BindingContext is NurseProfileViewModel vm)
                    {
                        await vm.RefreshAsync();
                    }
                });
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}