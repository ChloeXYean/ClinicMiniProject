using System.Collections.ObjectModel;
using System.Windows.Input;
using ClinicMiniProject.Models; 

namespace ClinicMiniProject.ViewModels
{
    public class EndConsultationViewModel : BindableObject
    {
        //public ObservableCollection<Appointment> ActiveConsultations { get; set; } = new();

        public ICommand BackCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public EndConsultationViewModel()
        {
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ViewDetailsCommand = new Command(OnViewDetails);

            // LoadData(); 
        }

        private async void OnViewDetails()
        {
            await Application.Current.MainPage.DisplayAlert("Info", "Navigating to consultation details...", "OK");
            // await Shell.Current.GoToAsync("ConsultationDetailsPage");
        }
    }
}