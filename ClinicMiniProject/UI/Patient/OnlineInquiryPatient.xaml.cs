using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Patient;

public partial class OnlineInquiryPatient : ContentPage
{
    public OnlineInquiryPatient()
    {
        InitializeComponent();

        // Get ViewModel from dependency injection
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var viewModel = sp?.GetService<OnlineMedicalInquiryViewModel>();
        BindingContext = viewModel;
    }
}