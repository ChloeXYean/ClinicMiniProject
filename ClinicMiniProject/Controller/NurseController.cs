using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Modules
{
    internal class NurseController
    {
        public List<Appointment> appointments = new List<Appointment>();
        public List<Patient> patients = new List<Patient>();
        public List<Appointment> ViewAppointmentList(DateTime selectedDate)
        {
            return appointments.FindAll(a => a.appointedAt.Date == selectedDate).OrderBy(a => a.appointedAt).ToList();
        }

        //View appointment history 
        public List<Appointment> ViewAppointmentHistory(Patient patient)
        {
            
            return appointments.FindAll(a => a.patientIC == patient.patientIC).OrderByDescending(a => a.appointedAt).ToList();
        }


        //TODO: Need to modify this method to integrate with SystemUtils for timeslot assignment
        public void ManageEmergencyAppointment(Appointment emergencyAppointment)
        {
            emergencyAppointment.status = "Emergency";
            emergencyAppointment.bookedAt = DateTime.Now;
            foreach (var apt in appointments)
            {
                if (apt.appointedAt == emergencyAppointment.appointedAt && apt.appointmentID != emergencyAppointment.appointmentID)
                {
                    apt.status = "Rescheduled";
                    //Neeed to send msg to the patiet that got emergency case
                    apt.appointedAt = apt.appointedAt.AddMinutes(30); // Reschedule by 30 minutes for simplicity
                }
            }
            appointments.Add(emergencyAppointment);
        }

        public void RegisterWalkInPatient(Patient patient)
        {
            var existingPatient = patients.Find(p => p.patientIC == patient.patientIC);
            if (existingPatient == null)
            {
                //Validate patient details here 
                patients.Add(patient);
            }
            else
            {
                //ady registered
            }




        }
    }
}
