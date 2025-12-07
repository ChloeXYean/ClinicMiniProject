using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class Patient
    {
        public string patient_IC { get; set; } = string.Empty;
        public string patient_name { get; set; } = string.Empty;
        public string patient_contact { get; set; } = string.Empty;
        public string patient_email { get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;
        public bool isAppUser { get; set; }
    }
}
