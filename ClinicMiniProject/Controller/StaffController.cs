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
            return appointments.FindAll(a => a.appointedAt.HasValue && a.appointedAt.Value.Date == selectedDate).OrderBy(a => a.appointedAt).ToList();
        }

        public void ManageDoctorAvailability(string docID, string day)
        {
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
            apt.appointment_status = "Completed";
            apt.consultation_status = "Done";
            apt.payment_status = "Pending";
        }


        public void UpdateDoctorAvailability(string doctorId, DayOfWeek dayOfWeek, bool working)
        {
            var doc = doctorAvailabilities
                .FirstOrDefault(a => a.staff_ID == doctorId);

            if (doc == null) throw new ArgumentException("Doctor not found.");

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: doc.Monday = working; break;
                case DayOfWeek.Tuesday: doc.Tuesday = working; break;
                case DayOfWeek.Wednesday: doc.Wednesday = working; break;
                case DayOfWeek.Thursday: doc.Thursday = working; break;
                case DayOfWeek.Friday: doc.Friday = working; break;
                case DayOfWeek.Saturday: doc.Saturday = working; break;
                case DayOfWeek.Sunday: doc.Sunday = working; break;
            }
        }

    }
}
