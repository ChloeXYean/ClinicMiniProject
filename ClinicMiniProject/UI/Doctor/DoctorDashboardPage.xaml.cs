using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

public partial class DoctorDashboardPage : ContentPage
{
    public DoctorDashboardPage()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<DoctorDashboardViewModel>();
        if (vm != null)
            BindingContext = vm;
    }

    public DoctorDashboardPage(DoctorDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
