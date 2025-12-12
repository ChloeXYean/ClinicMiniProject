using ClinicMiniProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Controller
{
    internal class StaffController  
    {
        public List<Appointment> appointments = new List<Appointment>();
        public List<DoctorAvailability> doctorAvailabilities = new List<DoctorAvailability>();
        public List<Appointment> ViewAppointmentList(DateTime selectedDate)
        {
            return appointments.FindAll(a => a.appointedAt.Date == selectedDate).OrderBy(a => a.appointedAt).ToList();
        }

        //Manage doctor availability - edit/update which day he got work, no job time, fix job time ady 
        public void ManageDoctorAvailability(string docID, string day)
        {
            //After select a doc, then choose which day (Mon - Sun) to edit availability
            //Monday: true/false
            var availability = doctorAvailabilities.FirstOrDefault(d => d.staff_ID == docID);
            if (availability == null)
            {
                throw new ArgumentException("Doctor not found.");
            }

            switch (day)
            {
                case "Monday":
                    availability.Monday = !availability.Monday;
                    break;
                case "Tuesday":
                    availability.Tuesday = !availability.Tuesday;
                    break;
                case "Wednesday":
                    availability.Wednesday = !availability.Wednesday;
                    break;
                case "Thursday":
                    availability.Thursday = !availability.Thursday;
                    break;
                case "Friday":
                    availability.Friday = !availability.Friday;
                    break;
                case "Saturday":
                    availability.Saturday = !availability.Saturday;
                    break;
                case "Sunday":
                    availability.Sunday = !availability.Sunday;
                    break;
                default:
                    throw new ArgumentException("Invalid day provided.");
            }

        }

        public void UpdateAppointmentStatus(Appointment apt)
        {
            //Get the appointment from the list, after select then come to here 
            //Press Done Consultation
            apt.appointment_status = "Completed";
            apt.consultation_status = "Done";
            apt.payment_status = "Pending";
        }


        public void UpdateDoctorAvailability(string doctorId, DayOfWeek dayOfWeek, bool working)
        {
            var availableList = doctorAvailabilities.FirstOrDefault( a => a.staff_ID == doctorId && a.DayOfWeek == dayOfWeek );
        } 
        //Payment staus update on Nurse controller

    }
}
