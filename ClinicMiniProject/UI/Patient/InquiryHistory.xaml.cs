namespace ClinicMiniProject.UI.Patient;

public partial class InquiryHistory : ContentPage
{
    private readonly ClinicMiniProject.ViewModels.OnlineMedicalInquiryViewModel? _viewModel;

    public InquiryHistory()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        _viewModel = sp?.GetService<ClinicMiniProject.ViewModels.OnlineMedicalInquiryViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Reload inquiry history when page appears
        if (_viewModel != null)
        {
            try
            {
                await _viewModel.LoadAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load inquiry history: {ex.Message}", "OK");
            }
        }
    }
}