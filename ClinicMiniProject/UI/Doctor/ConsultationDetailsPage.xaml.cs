using Microsoft.Extensions.DependencyInjection;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Doctor;

public partial class ConsultationDetailsPage : ContentPage, IQueryAttributable
{
    public ConsultationDetailsPage()
    {
        InitializeComponent();
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var vm = sp?.GetService<ConsultationDetailsViewModel>();
        if (vm != null)
            BindingContext = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("appointmentId") && BindingContext is ConsultationDetailsViewModel vm)
        {
            vm.AppointmentId = query["appointmentId"].ToString();
        }
    }
}