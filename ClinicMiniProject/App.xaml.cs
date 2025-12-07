using ClinicMiniProject.UI;

namespace ClinicMiniProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
