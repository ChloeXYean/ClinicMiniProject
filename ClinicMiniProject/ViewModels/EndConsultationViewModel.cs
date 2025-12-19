using ClinicMiniProject.Controller; // Import Controller
using ClinicMiniProject.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class EndConsultationViewModel : BindableObject
    {
        private readonly NurseController _controller; 
        public ObservableCollection<ConsultationQueueDto> ActiveConsultations { get; set; } = new();

        public ICommand BackCommand { get; }
        public ICommand CompleteCommand { get; } 

        public EndConsultationViewModel(NurseController controller) 
        {
            _controller = controller;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            CompleteCommand = new Command<ConsultationQueueDto>(async (item) => await OnComplete(item));

            LoadConsultations();
        }

        public async void LoadConsultations()
        {
            var appointments = await _controller.GetUpcomingAppointment();

            ActiveConsultations.Clear();

            foreach (var app in appointments)
            {
                var patient = _controller.ViewPatientDetails(app.patient_IC);
                string nameToDisplay = patient != null ? patient.patient_name : "Unknown";

                ActiveConsultations.Add(new ConsultationQueueDto
                {
                    ConsultationId = app.appointment_ID,
                    PatientName = nameToDisplay,
                    PatientIC = app.patient_IC,
                    ServiceType = app.service_type ?? "General",

                    Date = app.appointedAt?.ToString("dd MMM yyyy") ?? "-",
                    AppointedTime = app.appointedAt?.ToString("hh:mm tt") ?? "-"
                });
            }
        }

        private async Task OnComplete(ConsultationQueueDto item)
        {
            if (item == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm", $"End consultation for {item.PatientName}?", "Yes", "No");
            if (!confirm) return;

            bool success = await _controller.CompleteAppointment(item.ConsultationId);

            if (success)
            {
                ActiveConsultations.Remove(item);
                await Shell.Current.DisplayAlert("Success", "Consultation ended.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update status.", "OK");
            }
        }
    }
}