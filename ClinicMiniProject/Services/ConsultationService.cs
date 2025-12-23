using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Dtos;

namespace ClinicMiniProject.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly IAppointmentService _appointmentService;

        public ConsultationService(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        public async Task<IEnumerable<ConsultationQueueDto>> GetConsultationQueueAsync(string doctorId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            // Fetch appointments for the specific date
            var appointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, start, end);

            if (appointments == null) return Enumerable.Empty<ConsultationQueueDto>();

            var queue = new List<ConsultationQueueDto>();

            foreach (var appt in appointments
                    .Where(a => a.status != "Cancelled" && a.status != "Completed")
                    .OrderBy(a => a.appointedAt))
            {
                var patient = await _appointmentService.GetPatientByIcAsync(appt.patient_IC);

                queue.Add(new ConsultationQueueDto
                {
                    ConsultationId = appt.appointment_ID,
                    PatientName = patient?.patient_name ?? "Unknown",
                    PatientIC = appt.patient_IC,
                    ServiceType = appt.service_type.ToString(),
                    AppointedTime = appt.appointedAt?.ToString("h:mm tt") ?? "-",
                    Date = appt.appointedAt?.ToString("dd/MM/yyyy") ?? "-",
                    DoctorRemark = appt.doc_remark,
                    NurseRemark = appt.nurse_remark
                });
            }

            return queue;
        }
        public async Task<ConsultationDetailsDto?> GetCurrentConsultationDetailsAsync(string doctorId, DateTime now)
        {
            var today = now.Date;
            var endOfDay = today.AddDays(1);

            var todayAppointments = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, today, endOfDay))
                ?.ToList() ?? new List<Appointment>();

            // 1. Sort by Time (Earliest first) so 3 PM comes before 4 PM
            var current = todayAppointments
                .Where(a => a.appointedAt.Value.Date == today && (a.status == "Pending" || a.status == "Confirmed" || a.status == "InProgress"))
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
        public async Task UpdateRemarksAsync(string appointmentId, string docRemark, string nurseRemark)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appt != null)
            {
                appt.doc_remark = docRemark;
                appt.nurse_remark = nurseRemark;

                await _appointmentService.UpdateAppointmentAsync(appt);
            }
        }

        public async Task UpdateConsultationRemarksAsync(string appointmentId, string newRemarks)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appt != null)
            {
                appt.doc_remark = newRemarks;
                await _appointmentService.UpdateAppointmentAsync(appt);
            }
        }
    }
}
