using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Modules
{
    public class Patient
    {
        public string patientIC { get; set; }
        public string patientName { get; set; }
        public string patientContact { get; set; }
        public string patientEmail { get; set; }
        public bool isAppUser { get; set; }
    }
}
