using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ClinicMiniProject.Controller
{
    internal class ReportingController
    {
        //Get date/time 
        //Select Service Type
        public void ReportSelection()
        {
            string [] reportType = { "General Consultation", "Follow up treatment", "Test Result Discussion", "Vaccination/Injection", "Medical Screening(blood test, blood pressure, sugar test)" };
            for (int i = 0; i < reportType.Length; i++)
            {
                //Display
                Console.WriteLine($"{i + 1}. {reportType[i]}");
            }
            //onClick or something


        }
    }
}
