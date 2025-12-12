using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class Appointment
    {
        public string appointment_ID { get; set; }
        public DateTime bookedAt { get; set; }
        public DateTime appointedAt { get; set; }
        public string appointment_ID { get; set; }
        public string patient_IC { get; set; }
        public string appointment_status { get; set; }
        public string staff_ID { get; set; }
        public string patient_IC { get; set; }
        public string appointment_status { get; set; }
        public string consultation_status { get; set; }
        public string payment_status { get; set; }
        public string pickup_status { get; set; }    
    }
}