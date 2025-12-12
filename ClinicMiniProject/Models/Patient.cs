using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class Patient
    {
        public string patient_IC { get; set; }
        public string patient_name { get; set; }
        public string patient_contact { get; set; }

        public string password { get; set; }
        public string patient_email { get; set; }
        public bool isAppUser { get; set; }
    }
}
