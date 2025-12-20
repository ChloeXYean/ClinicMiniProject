using ClinicMiniProject.ViewModels;
namespace ClinicMiniProject.UI.Patient;

public partial class AppointmentBooking_Patient : ContentPage
{
    public AppointmentBooking_Patient(PatientAppointmentBookingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}