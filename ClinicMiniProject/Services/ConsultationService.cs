using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly IAppointmentService _appointmentService;

        public ConsultationService(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<ConsultationDetailsDto?> GetCurrentConsultationDetailsAsync(string doctorId, DateTime now)
        {
            var today = now.Date;
            var endOfDay = today.AddDays(1);

            var todayAppointments = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, today, endOfDay))
                ?.ToList() ?? new List<Appointment>();

            // Define "current time slot" as the earliest appointment that is not completed and is scheduled for today.
            // If you have a fixed slot system, replace this selection logic.
            var current = todayAppointments
                .Where(a => a.appointedAt.Value.Date == today && (a.status == "Scheduled" || a.status == "Pending" || a.status == "Confirmed"))
                .OrderBy(a => a.appointedAt)
                .FirstOrDefault();

            if (current == null)
                return null;

            // 15-minute rule:
            // If appointment start time + 15 mins has passed and doctor hasn't started consultation, cancel and reassign walk-in.
            // We treat "Started" as status == "InProgress".
            var isStarted = string.Equals(current.status, "InProgress", StringComparison.OrdinalIgnoreCase);
            if (!isStarted && now >= current.appointedAt.AddMinutes(15))
            {
                await CancelAndReassignToRandomWalkInAsync(current, now);

                // After reassignment, rebuild details using updated appointment.
                var updated = await _appointmentService.GetAppointmentByIdAsync(current.appointmentID);
                if (updated == null)
                    return null;

                return await BuildDetailsAsync(updated);
            }

            return await BuildDetailsAsync(current);
        }

        public async Task<IEnumerable<PatientLookupDto>> SearchPatientsByNameAsync(string nameQuery)
        {
            var patients = (await _appointmentService.SearchPatientsByNameAsync(nameQuery))?.ToList() ?? new List<Patient>();
            return patients.Select(p => new PatientLookupDto
            {
                PatientIc = p.patient_IC,
                PatientName = p.patient_name,
                PatientPhone = p.patient_contact
            });
        }

        public async Task<ConsultationDetailsDto?> GetConsultationDetailsByAppointmentIdAsync(string appointmentId)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appt == null)
                return null;

            return await BuildDetailsAsync(appt);
        }

        public async Task StartConsultationAsync(string appointmentId)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appt == null)
                throw new InvalidOperationException("Appointment not found.");

            appt.status = "InProgress";

            // TODO: link with database (persist appointment status)
            await _appointmentService.UpdateAppointmentAsync(appt);
        }

        public async Task EndConsultationAsync(string appointmentId, string remark)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appt == null)
                throw new InvalidOperationException("Appointment not found.");

            appt.status = "Completed";

            // TODO: link with database (persist appointment status)
            await _appointmentService.UpdateAppointmentAsync(appt);

            // TODO: link with database
            // Persist remark to a Consultation/MedicalRecord table.
            // Current Models do not include remark, so we do NOT store it here.
            _ = remark;
        }

        private async Task CancelAndReassignToRandomWalkInAsync(Appointment appointment, DateTime now)
        {
            appointment.status = "Cancelled";

            // TODO: link with database (persist cancellation)
            await _appointmentService.UpdateAppointmentAsync(appointment);

            var walkIn = await _appointmentService.GetRandomWalkInPatientAsync();
            if (walkIn == null)
            {
                // No walk-in patient available; leave as cancelled.
                return;
            }

            // Reassign same slot to a walk-in patient.
            var reassigned = new Appointment
            {
                appointment_ID = appointment.appointment_ID,
                bookedAt = now,
                appointedAt = appointment.appointedAt,
                staff_ID = appointment.staff_ID,
                patient_IC = walkIn.patient_IC,
                status = "Scheduled"
            };

            // TODO: link with database (persist reassignment)
            await _appointmentService.UpdateAppointmentAsync(reassigned);
        }

        private async Task<ConsultationDetailsDto> BuildDetailsAsync(Appointment appt)
        {
            var patient = await _appointmentService.GetPatientByIcAsync(appt.patient_IC);

            return new ConsultationDetailsDto
            {
                AppointmentId = appt.appointment_ID,
                Date = appt.appointedAt.Value.Date,
                AppointedTimeSlot = appt.appointedAt.Value,
                PatientIc = appt.patient_IC,
                PatientName = patient?.patient_name ?? string.Empty,
                PatientPhone = patient?.patient_contact ?? string.Empty,
                SelectedServiceType = string.Empty, // TODO: link with database (Appointment currently has no service type)
                Status = appt.status
            };
        }
    }
}
