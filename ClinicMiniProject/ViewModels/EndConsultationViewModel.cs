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

        public EndConsultationViewModel(NurseController nurseController)
        {
            _nurseController = nurseController;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // This command runs when you click "End Consultation" link in the UI
            ViewDetailsCommand = new Command<ConsultationQueueDto>(OnCompleteConsultation);

            // Load real data immediately
            LoadConsultations();
        }

        private async void LoadConsultations()
        {
            // You might want to create a method in NurseController specifically for this
            // For now, we reuse the "GetWalkInQueue" or fetching "Pending/Scheduled" logic
            // Ideally, you want appointments where status == "In Progress" or "Pending"

            // Example Logic:
            var appointments = await _nurseController.GetWalkInQueueForToday();
            // Note: You might need to adjust the Controller to return 'Pending' AND 'Scheduled' appointments here.

            ActiveConsultations.Clear();
            foreach (var app in appointments)
            {
                // Map the PatientQueueDto or Appointment to ConsultationQueueDto
                ActiveConsultations.Add(new ConsultationQueueDto
                {
                    PatientName = app.PatientName,
                    ConsultationId = app.QueueId, // Using QueueID/ApptID as ID
                    ServiceType = "General Checkup", // Or fetch real service type
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
                { "SelectedConsultation", consultation }
            };

            // Navigate to the details page (Ensure this route is registered in AppShell!)
            await Shell.Current.GoToAsync("ConsultationDetails", navParam);
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