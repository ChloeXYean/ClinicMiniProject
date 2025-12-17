using ClinicMiniProject.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class PatientHomeViewModel : BindableObject
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthService _authService;

        public ICommand HomeCommand { get; }
        public ICommand InquiryHistoryCommand { get; }
        public ICommand ProfileCommand { get; }

        public 
    }
}
