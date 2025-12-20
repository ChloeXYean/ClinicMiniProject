namespace ClinicMiniProject.UI.Nurse;

public partial class EndConsultationDetailsPage : ContentPage
{
    public EndConsultationDetailsPage()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services;
        if (sp != null)
        {
            BindingContext = sp.GetService<ViewModels.EndConsultationDetailsViewModel>();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}