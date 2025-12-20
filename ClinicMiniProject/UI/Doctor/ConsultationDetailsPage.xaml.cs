using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.Services.Interfaces; 

namespace ClinicMiniProject.UI.Doctor;

public partial class ConsultationDetailsPage : ContentPage
{
    public ConsultationDetailsPage()
    {
        InitializeComponent();
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<ConsultationDetailsViewModel>();
        if (vm != null)
            BindingContext = vm;
    }
}