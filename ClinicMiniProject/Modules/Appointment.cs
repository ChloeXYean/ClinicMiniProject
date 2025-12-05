using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Modules
{
    public class Appointment
    {
        public string appointmentID { get; set; }
        public DateTime bookedAt { get; set; }
        public DateTime appointedAt { get; set; }
        public string staffID { get; set; }
        public string patientIC { get; set; }
        public string status { get; set; }
    }
}
