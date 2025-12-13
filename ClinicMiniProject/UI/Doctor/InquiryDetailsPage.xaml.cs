using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

[QueryProperty(nameof(InquiryId), "inquiryId")]
public partial class InquiryDetailsPage : ContentPage
{
    private string _inquiryId = string.Empty;

    public string InquiryId
    {
        get => _inquiryId;
        set
        {
            _inquiryId = value;
            if (BindingContext is InquiryDetailsViewModel vm)
                vm.InquiryId = value;
        }
    }

    public InquiryDetailsPage()
    {
        InitializeComponent();
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<InquiryDetailsViewModel>();
        if (vm != null)
            BindingContext = vm;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
