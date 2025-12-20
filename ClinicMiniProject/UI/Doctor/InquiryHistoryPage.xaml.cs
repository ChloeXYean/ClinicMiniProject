using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

public partial class InquiryHistoryPage : ContentPage
{
    private readonly DoctorDashboardViewModel _viewModel;

    public InquiryHistoryPage(DoctorDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(DoctorDashboardPage)}");
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Search text changed: '{e.NewTextValue}'");
        
        if (BindingContext is DoctorDashboardViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"Calling FilterInquiries with {vm.AllInquiries.Count} total inquiries");
            vm.FilterInquiries(e.NewTextValue);
            System.Diagnostics.Debug.WriteLine($"After filtering: {vm.FilteredInquiries.Count} inquiries remaining");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("BindingContext is not DoctorDashboardViewModel");
        }
    }
}
