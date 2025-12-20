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

            // 1. Sort by Time (Earliest first) so 3 PM comes before 4 PM
            var current = todayAppointments
                .Where(a => a.appointedAt.Value.Date == today && (a.status == "Pending" || a.status == "Confirmed" || a.status == "Pending"))
                .OrderBy(a => a.appointedAt)
                .FirstOrDefault();

            if (current == null) return null;
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

        // 1. Update signature to accept 'nurseRemark'
        public async Task EndConsultationAsync(string appointmentId, string doctorRemark, string? nurseRemark)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appt == null)
                throw new InvalidOperationException("Appointment not found.");

            appt.status = "Completed";

            appt.doc_remark = doctorRemark;
            appt.nurse_remark = nurseRemark;

            await _appointmentService.UpdateAppointmentAsync(appt);
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
                status = "Pending"
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
                Status = appt.status,
                DoctorRemark = appt.doc_remark ?? string.Empty,
                NurseRemark = appt.nurse_remark ?? string.Empty
            };
        }
    }
}
