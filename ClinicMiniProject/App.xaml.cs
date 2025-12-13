using ClinicMiniProject.UI;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new UITestPage("Test user");
        }
    }
}

