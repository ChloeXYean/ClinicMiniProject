using ClinicMiniProject.Dtos;
using ClinicMiniProject.Models; 
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class EndConsultationViewModel : BindableObject
    {
        public ObservableCollection<ConsultationQueueDto> ActiveConsultations { get; set; } = new();

        public ICommand BackCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public EndConsultationViewModel()
        {
            // Dummy Data - deleteable
            ActiveConsultations = new ObservableCollection<ConsultationQueueDto>
            {
                new ConsultationQueueDto { PatientName = "John Doe", EndTime = "10:30 AM", ConsultationId = "C001" },
                new ConsultationQueueDto { PatientName = "Jane Smith", EndTime = "11:00 AM", ConsultationId = "C002" }
            };
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ViewDetailsCommand = new Command<ConsultationQueueDto>(OnViewDetails);
        }

        private async void OnViewDetails(ConsultationQueueDto consultation)
        {
            if (consultation == null) return;

            var navParam = new Dictionary<string, object>
            {
                { "SelectedConsultation", consultation }
            };

            await Shell.Current.GoToAsync("ConsultationDetails", navParam);
        }
    }
}