using ClinicMiniProject.Dtos;
using ClinicMiniProject.Models; 
using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Controller;

namespace ClinicMiniProject.ViewModels
{
    public class EndConsultationViewModel : BindableObject
    {
        private readonly NurseController _nurseController; 

        public ObservableCollection<ConsultationQueueDto> ActiveConsultations { get; set; } = new();

        public ICommand BackCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public ICommand EndSessionCommand { get; }

        public EndConsultationViewModel(NurseController nurseController)
        {
            _nurseController = nurseController;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            ViewDetailsCommand = new Command<ConsultationQueueDto>(OnViewDetails);

            EndSessionCommand = new Command<ConsultationQueueDto>(OnCompleteConsultation);
            LoadConsultations();
        }

        private async void LoadConsultations()
        {
            var appointments = await _nurseController.GetWalkInQueueForToday();

            ActiveConsultations.Clear();
            foreach (var app in appointments)
            {
                ActiveConsultations.Add(new ConsultationQueueDto
                {
                    PatientName = app.PatientName,
                    ConsultationId = app.QueueId,

                    ServiceType = app.ServiceType,

                    AppointedTime = app.RegisteredTime,
                    Date = DateTime.Today.ToString("dd MMM yyyy"), 
                    PatientIC = app.ICNumber
                });
            }
        }

        private async void OnViewDetails(ConsultationQueueDto consultation)
        {
            if (consultation == null) return;

            var navParam = new Dictionary<string, object>
            {
                { "AppointmentId", consultation.ConsultationId }
            };

            await Shell.Current.GoToAsync("EndConsultationDetails", navParam);
        }

        private async void OnCompleteConsultation(ConsultationQueueDto consultation)
        {
            if (consultation == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm", $"End consultation for {consultation.PatientName}?", "Yes", "No");
            if (!confirm) return;

            // Call the Controller to update DB
            bool success = await _nurseController.CompleteAppointment(consultation.ConsultationId);

            if (success)
            {
                ActiveConsultations.Remove(consultation);
                await Shell.Current.DisplayAlert("Success", "Consultation ended.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update status.", "OK");
            }
        }
    }
}