using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class UpcomingAppointmentView
    {
        public string Time { get; set; }
        public string DoctorName { get; set; }

        public int ConsultationQueueCount { get; set; }
        public int PaymentQueueCount { get; set; }
        public int PickupQueueCount { get; set; }

        public string ConsultationStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PickupStatus { get; set; }
    }
}
