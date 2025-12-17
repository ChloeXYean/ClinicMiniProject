using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Doctor;

[QueryProperty(nameof(AppointmentId), "appointmentId")]
public partial class ConsultationDetailsPage : ContentPage
{
    private readonly ConsultationDetailsViewModel _viewModel;

    private string _appointmentId = string.Empty;

    public string AppointmentId
    {
        get => _appointmentId;
        set
        {
            _appointmentId = value;
            if (BindingContext is ConsultationDetailsViewModel vm)
                vm.AppointmentId = value;
        }
    }

    public ConsultationDetailsPage(ConsultationDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
