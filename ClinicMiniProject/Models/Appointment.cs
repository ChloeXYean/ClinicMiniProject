using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class Appointment
    {
        public string appointmentID { get; set; } = string.Empty;
        public DateTime bookedAt { get; set; }
        public DateTime appointedAt { get; set; }
        public string staff_ID { get; set; } = string.Empty;
        public string patient_IC { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
}
