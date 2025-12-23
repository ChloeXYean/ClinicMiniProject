using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ClinicMiniProject.ViewModels
{
    public class PatientConsultationDetailsViewModel : BindableObject
    {
        public ICommand BackCommand { get; }

        public PatientConsultationDetailsViewModel()
        {
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }
    }
}