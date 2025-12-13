using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services;

namespace ClinicMiniProject.Controller
{
    internal class NurseController
    {
        public List<Appointment> appointments { get; private set; }
        public List<DocAvailable> doctorAvailabilities { get; private set; } 
        public List<Patient> patients { get; private set; }
        public List<Staff> staffs { get; private set; }

        private readonly AppointmentService _aptService;
        private readonly PatientService _patientService;

        public List<Appointment> ViewAppointmentList(DateTime selectedDate)
        {
            if (appointments == null || appointments.Count == 0)
                return new List<Appointment>();

            return appointments
                .Where(a => a.appointedAt.HasValue && a.appointedAt.Value.Date == selectedDate.Date)
                .OrderBy(a => a.appointedAt.Value) 
                .ToList();
        }

        public List<Appointment> ViewAppointmentHistory(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            return appointments
                .Where(a => a.patient_IC == patient.patient_IC)
                .OrderByDescending(a => a.appointedAt ?? DateTime.MinValue)
                .ToList();
        }

        public void ManageEmergencyAppointment(Appointment emergencyAppointment, int shiftMinutes = 60)
        {
            if (emergencyAppointment == null) throw new ArgumentNullException(nameof(emergencyAppointment));

            emergencyAppointment.appointment_status = "Emergency";
            emergencyAppointment.bookedAt = DateTime.Now;

            if (!emergencyAppointment.appointedAt.HasValue)
            {
                appointments.Add(emergencyAppointment);
                return;
            }

            var emergencyStart = emergencyAppointment.appointedAt.Value;
            var emergencyEnd = emergencyStart.AddMinutes(shiftMinutes);

            var conflicts = appointments
                .Where(apt => apt.appointedAt.HasValue)
                .Where(apt =>
                {
                    var start = apt.appointedAt.Value;
                    var end = start.AddMinutes(shiftMinutes);
                    return !(emergencyEnd <= start || emergencyStart >= end);
                })
                .ToList();

            foreach (var apt in conflicts)
            {
                apt.appointment_status = "Rescheduled";
                apt.appointedAt = apt.appointedAt.Value.AddMinutes(shiftMinutes);
                // TODO: notify patient about reschedule
            }

            appointments.Add(emergencyAppointment);
        }

        public Appointment RegisterWalkInPatient(Patient patient, string docName = null)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            _patientService.AddPatient(patient);

            string? assignedDoctorId = null;
            if (!string.IsNullOrWhiteSpace(docName) && staffs != null)
            {
                var doc = staffs.FirstOrDefault(s =>
                    string.Equals(s.staff_name, docName, StringComparison.OrdinalIgnoreCase));
                assignedDoctorId = doc?.staff_ID;
            }

            DateTime preferredDate = DateTime.Today;
            DateTime? slot = null;

            //if got specific doc
            if (!string.IsNullOrEmpty(assignedDoctorId))
            {
                slot = _aptService.AssignWalkInTimeSlot(assignedDoctorId, preferredDate);
                if (slot == null)
                {
                    //unavailable 
                    assignedDoctorId = null;
                }   
            }
            //No doc
            if (string.IsNullOrEmpty(assignedDoctorId))
            {
                if (doctorAvailabilities != null)
                {
                    var availableDoc = doctorAvailabilities
                        .Where(a => a.IsAvailable(preferredDate.DayOfWeek))
                        .OrderBy(_ => Random.Shared.Next()) // random order
                        .ToList();

                    foreach (var doc in availableDoc)
                    {
                        var trySlot = _aptService.AssignWalkInTimeSlot(doc.staff_ID, preferredDate);
                        if (trySlot != null)
                        {
                            assignedDoctorId = doc.staff_ID;
                            slot = trySlot;
                            break;
                        }
                    }
                }
            }

            var appt = new Appointment
            {
                patient_IC = patient.patient_IC,
                staff_ID = assignedDoctorId ?? string.Empty,
                appointedAt = slot,
                appointment_status = slot.HasValue ? "Pending" : "NoSlot"
            };

            _patientService.AddAppointment(appt);

            return appt;
        }


        public List<Appointment> EndDocConsultation() 
        {
            return appointments.Where(a => a.appointment_status == "Completed").OrderByDescending(a => a.appointedAt).ToList();
        }

        public void UpdatePaymentStatus(Appointment apt)
        {
            apt.payment_status = "Done";
        }

        public Patient ViewPatientDetails(string patientIC)
        {
            var patient = patients.FirstOrDefault(p => p.patient_IC != patientIC);
            if (patient == null)
                {
                throw new ArgumentException("Patient not found.");
            }
            return patient;

        }

        

    }
}
