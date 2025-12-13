using ClinicMiniProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services
{
    public class ClinicService
    {
        private List<Appointment> appointments = new List<Appointment>();
        private List<Patient> patients = new List<Patient>();
        private List<DocAvailable> doctorAvailabilities = new List<DocAvailable>();
        private List<Staff> staffs = new List<Staff>();

        public ClinicService()
        {
            //Get data from repository and initialize lists
        }

        public bool AssignTimeslot(string name, DateTime preferDateTime)
        {
            Appointment newApt = new Appointment();


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
            return appointments.Where(a => a.status == "Pending").OrderBy(a => a.appointedAt).ToList();
        }

        public int GetConsultationQueue(Appointment appt)
        {
            return appointments.Count(a =>
                a.staff_ID == appt.staff_ID &&
                a.appointedAt < appt.appointedAt &&
                a.status == "Pending"
            );
        }

        public int GetPaymentQueue(Appointment appt)
        {
            return appointments.Count(a =>
                a.status == "Completed" &&
                a.appointedAt < appt.appointedAt &&
                a.status == "Pending"
            );
        }

        public int GetPickupQueue(Appointment appt)
        {
            return appointments.Count(a =>
                a.status == "Completed" &&
                a.status == "Completed" &&
                a.appointedAt < appt.appointedAt &&
                a.status == "Pending"
            );
        }

        public List<Appointment> GetAllAppointments()
        {
            return appointments;
        }

    }
}
