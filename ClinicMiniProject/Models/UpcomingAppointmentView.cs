using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class UpcomingAppointmentView
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string DoctorName { get; set; }
        public string ConsultationRoom { get; set; }
        public string PaymentCounter { get; set; }
        public string PickupCounter { get; set; }
    }
}
