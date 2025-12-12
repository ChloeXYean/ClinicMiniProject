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
        private List<DoctorAvailability> doctorAvailabilities = new List<DoctorAvailability>();
        private List<Staff> staffs = new List<Staff>();

        public ClinicService()
        {
            //Get data from repository and initialize lists
        }

        public bool AssignTimeslot(string name, DateTime preferDateTime)
        {
            //string id = GetStaffIdByName(name);
            string id = null;
            Appointment newApt = null;


            bool isAvailable = appointments.Any(a => a.appointedAt == preferDateTime);
            //extract workday (Monday?..Friday) from preferDateTime
            string day = preferDateTime.DayOfWeek.ToString();
            //for each, Monday, Tuesday, .. Friday, check if doctor is available on that day
            for (int i = 0; i < doctorAvailabilities.Count; i++)
            {
                var docAvail = doctorAvailabilities[i];
                if (docAvail.staff_ID == id)
                {
                    bool isWorkDay = day switch
                    {
                        "Monday" => docAvail.Monday,
                        "Tuesday" => docAvail.Tuesday,
                        "Wednesday" => docAvail.Wednesday,
                        "Thursday" => docAvail.Thursday,
                        "Friday" => docAvail.Friday,
                        "Saturday" => docAvail.Saturday,
                        "Sunday" => docAvail.Sunday,
                        _ => false
                    };
                    if (!isWorkDay)
                    {
                        return false; // Doctor not available on that day
                    }
                    isAvailable = !isAvailable;
                    break;
                }
            }
            if (isAvailable)
            {
                newApt.appointment_status = "Pending";
                newApt.appointedAt = preferDateTime;
                appointments.Add(newApt);
                return true;
            }
            return false;
        }

        public List<Appointment> GetQueueByStatus(string status)
        {
            return appointments.Where(a => a.appointment_status == "Pending").OrderBy(a => a.appointedAt).ToList();
        }

        public int GetConsultationQueue(Appointment appt)
        {
            return appointments.Count(a =>
                a.staff_ID == appt.staff_ID &&
                a.appointedAt < appt.appointedAt &&
                a.appointment_status == "Pending"
            );
        }

        public int GetPaymentQueue(Appointment appt)
        {
            return appointments.Count(a =>
                a.consultation_status == "Completed" &&
                a.appointedAt < appt.appointedAt &&
                a.appointment_status == "Pending"
            );
        }

        public int GetPickupQueue(Appointment appt)
        {
            return appointments.Count(a =>
                a.consultation_status == "Completed" &&
                a.payment_status == "Completed" &&
                a.appointedAt < appt.appointedAt &&
                a.appointment_status == "Pending"
            );
        }


    }
}
