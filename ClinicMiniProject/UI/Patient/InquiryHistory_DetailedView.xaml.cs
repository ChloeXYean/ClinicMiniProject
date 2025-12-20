using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Patient;

[QueryProperty(nameof(InquiryId), "inquiryId")]
public partial class InquiryHistory_DetailedView : ContentPage
{
    public string InquiryId
    {
        set
        {
            if (BindingContext is InquiryDetailsViewModel vm)
            {
                vm.InquiryId = value;
            }
        }
    }

    public InquiryHistory_DetailedView()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var viewModel = sp?.GetService<InquiryDetailsViewModel>();
        BindingContext = viewModel;
    }
}