using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Patient;

public partial class SelectDoctorPage : ContentPage
{
    public SelectDoctorPage(SelectDoctorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
