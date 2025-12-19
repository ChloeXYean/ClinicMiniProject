using Appointment = ClinicMiniProject.Models.Appointment;
using DocAvailable = ClinicMiniProject.Models.DocAvailable;
using Patient = ClinicMiniProject.Models.Patient;
using Staff = ClinicMiniProject.Models.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services
{
    public class ClinicService
    {
        private List<ClinicMiniProject.Models.Appointment> appointments = new List<ClinicMiniProject.Models.Appointment>();
        private List<Patient> patients = new List<Patient>();
        private List<DocAvailable> doctorAvailabilities = new List<DocAvailable>();
        private List<Staff> staffs = new List<Staff>();

        public ClinicService()
        {
            //Get data from repository and initialize lists
        }

        public bool AssignTimeslot(string name, DateTime preferDateTime)
        {
            ClinicMiniProject.Models.Appointment newApt = new ClinicMiniProject.Models.Appointment();


            bool isAvailable = appointments.Any(a => a.appointedAt == preferDateTime);
            string day = preferDateTime.DayOfWeek.ToString();
            var availabilityList = doctorAvailabilities.FirstOrDefault(a => a.IsAvailable(preferDateTime.DayOfWeek));

            if (availabilityList == null)
            {
                newApt.status = "Pending";
                newApt.appointedAt = preferDateTime;
                appointments.Add(newApt);
                return true;
            }
            return false;
        }

        public List<Appointment> GetQueueByStatus(string status)
        {
            return appointments.Where(a => a.status == status).OrderBy(a => a.appointedAt).ToList();
        }


        public List<Appointment> GetAllAppointments()
        {
            return appointments;
        }

    }
}
