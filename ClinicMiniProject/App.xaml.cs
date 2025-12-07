using ClinicMiniProject.UI;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Ensure the application provides a startup UI.
            // Use AppShell if your project is structured around Shell.
            MainPage = new AppShell();
        }
    }
}

