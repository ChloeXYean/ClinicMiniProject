using ClinicMiniProject.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicMiniProject.UI.Doctor;

[QueryProperty(nameof(InquiryId), "inquiryId")]
[QueryProperty(nameof(UserType), "UserType")] 
public partial class InquiryDetailsPage : ContentPage
{
    private string _inquiryId = string.Empty;
    private string _userType = string.Empty; // Add this field

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

    // Add this property
    public string UserType
    {
        get => _userType;
        set
        {
            _userType = value;
            if (BindingContext is InquiryDetailsViewModel vm)
                vm.UserType = value;
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
}