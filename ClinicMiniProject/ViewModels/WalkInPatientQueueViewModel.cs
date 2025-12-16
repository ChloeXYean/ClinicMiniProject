using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class WalkInPatientQueueViewModel : BindableObject
    {
        // public ObservableCollection<PatientQueueItem> QueueList { get; set; } = new();

        public ICommand BackCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public WalkInPatientQueueViewModel()
        {
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ViewDetailsCommand = new Command(OnViewDetails);
        }

        private async void OnViewDetails()
        {
            await Application.Current.MainPage.DisplayAlert("Info", "Navigating to queue details...", "OK");
        }
    }
}