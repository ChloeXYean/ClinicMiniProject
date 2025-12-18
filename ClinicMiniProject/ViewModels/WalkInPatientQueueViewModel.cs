using ClinicMiniProject.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class WalkInPatientQueueViewModel : BindableObject
    {
        public ObservableCollection<PatientQueueDto> QueueList { get; set; } = new();

        public ICommand BackCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public WalkInPatientQueueViewModel()
        {
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ViewDetailsCommand = new Command<PatientQueueDto>(OnViewDetails);
        }

        private async void OnViewDetails(PatientQueueDto patient)
        {
            if (patient == null) return;

            // Navigate to PatientDetailsPage and pass the specific patient object
            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedPatient", patient }
            };
            await Shell.Current.GoToAsync("PatientDetails");
        }
    }
}