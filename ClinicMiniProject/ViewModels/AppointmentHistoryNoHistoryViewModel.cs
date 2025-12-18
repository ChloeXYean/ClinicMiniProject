using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class AppointmentHistoryNoHistoryViewModel : BindableObject
    {
        public ICommand BackCommand { get; }

        public AppointmentHistoryNoHistoryViewModel()
        {
            BackCommand = new Command(async () => await Shell.Current.GoToAsync("///PatientHomePage"));
        }
    }
}
