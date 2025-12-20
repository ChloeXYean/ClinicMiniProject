namespace ClinicMiniProject.UI.Nurse;

public partial class EndConsultationDetailsPage : ContentPage
{
    public EndConsultationDetailsPage()
    {
        InitializeComponent();
        // BindingContext will be set via DI or manually in AppShell

        // Manual DI resolution if not using MauiProgram builder for pages:
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