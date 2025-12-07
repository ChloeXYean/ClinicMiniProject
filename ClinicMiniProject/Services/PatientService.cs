using System;
using System.Collections.Generic;
using System.Linq;
using ClinicMiniProject.Models;
namespace ClinicMiniProject.Services
{

    public class PatientService
    {
        private List<Patient> patients = new List<Patient>();
        private List<Appointment> appointments = new List<Appointment>();
        private StaffService staffService;

        public PatientService(StaffService staffService)
        {
            this.staffService = staffService;
        }

        //Add a new patient
        public void AddPatient(Patient patient)
        {
            if (patient == null || string.IsNullOrWhiteSpace(patient.patient_IC))
            {
                Console.WriteLine("Invalid patient data.");
                return;
            }

            patients.Add(patient);
        }

        //Get patient by IC
        public Patient GetPatientByIC(string patient_IC)
        {
            return patients.FirstOrDefault(p => p.patient_IC == patient_IC);
        }

        //Get all patients
        public List<Patient> GetAllPatients()
        {
            return patients;
        }

        // Add appointment
        public void AddAppointment(Appointment appt)
        {
            appointments.Add(appt);
        }

        //---------------------------------------------------------------
        /*get upcoming appointment
         - date 
         - time 
         - doctor 
         - doctor consultation
         - payment counter
         - medical pickup counter */

        public Appointment GetUpcomingAppointmentEntity(string patient_IC)
        {
            return appointments
                .Where(a => a.patient_IC == patient_IC && a.status == "Pending") // return when IC correct and status is pending
                .OrderBy(a => a.appointedAt)
                .FirstOrDefault();
        } 

        public UpcomingAppointmentView GetUpcomingAppointmentDetails(string patient_IC)
        {
            var appt = GetUpcomingAppointmentEntity(patient_IC);
            if (appt == null)
            {
                return null; // No upcoming appointment
            }
            var doctor = staffService.GetStaffByID(appt.staff_ID);

            return new UpcomingAppointmentView
            {
                Date = appt.appointedAt.ToString("yyyy-MM-dd"),
                Time = appt.appointedAt.ToString("HH:mm"),
                DoctorName = doctor?.staff_name ?? "Unknown",
                doctorConsultation = appointments.Count(a => a.staff_ID == appt.staff_ID && a.appointedAt < appt.appointedAt && a.status == "Pending"),
                PaymentCounter = "Counter A",   // Placeholder
                PickupCounter = "Counter B"      // Placeholder
            };
        }


        //get appointment booking

        //get appointment history

        //get online inquiry

    }
}