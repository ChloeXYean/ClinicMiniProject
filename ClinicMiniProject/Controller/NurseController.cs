using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Controller
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
            
            return appointments.FindAll(a => a.patient_IC == patient.patient_IC).OrderByDescending(a => a.appointedAt).ToList();
        }


        //TODO: Need to modify this method to integrate with SystemUtils for timeslot assignment
        public void ManageEmergencyAppointment(Appointment emergencyAppointment)
        {
            emergencyAppointment.appointment_status = "Emergency";
            emergencyAppointment.bookedAt = DateTime.Now;
            foreach (var apt in appointments)
            {
                if (apt.appointedAt == emergencyAppointment.appointedAt && apt.appointment_ID != emergencyAppointment.appointment_ID)
                {
                    apt.appointment_status = "Rescheduled";
                    //Neeed to send msg to the patiet that got emergency case
                    apt.appointedAt = apt.appointedAt.AddMinutes(30); // Reschedule by 30 minutes for simplicity
                }
            }
            appointments.Add(emergencyAppointment);
        }

        public void RegisterWalkInPatient(Patient patient)
        {
            var existingPatient = patients.Find(p => p.patient_IC == patient.patient_IC);
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
