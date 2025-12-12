using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public class Staff
    {
        public string staff_ID { get; set; }

        public string staff_password { get; set; }
        public string staff_name { get; set; }
        public string staff_contact { get; set; }
        public string specialities { get; set; }
        public bool isDoctor { get; set; }
    }
}
