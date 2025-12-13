using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class AppointmentScheduleService : IAppointmentScheduleService
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentScheduleService(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<AppointmentScheduleGridDto> GetScheduleGridAsync(string doctorId, DateTime date)
        {
            var day = date.Date;
            var endOfDay = day.AddDays(1);

            // Service columns required by UI.
            // NOTE: current Appointment model doesn't have a service type field yet.
            // We default all appointments into "General Consultation" until DB/model supports it.
            var serviceTypes = new List<string>
            {
                "GeneralConsultation",
                "FollowUpTreatment",
                "TestResultDiscussion",
                "VaccinationInjection",
                "MedicalScreening"
            };

            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, day, endOfDay))
                ?.Where(a => a.appointedAt.HasValue)
                .ToList() ?? new();

            // Build 30-min slots from 09:00 to 21:00 (inclusive start, exclusive end).
            var start = day.AddHours(9);
            var end = day.AddHours(21);

            var rows = new List<TimeSlotRowDto>();
            for (var t = start; t < end; t = t.AddMinutes(30))
            {
                var row = new TimeSlotRowDto { SlotStart = t };

                foreach (var st in serviceTypes)
                {
                    row.CellsByServiceType[st] = new SlotCellDto { IsBooked = false };
                }

                rows.Add(row);
            }

            // Fill appointments into the grid.
            foreach (var appt in appts)
            {
                var slot = appt.appointedAt!.Value;

                // Find matching row
                var row = rows.FirstOrDefault(r => r.SlotStart == new DateTime(slot.Year, slot.Month, slot.Day, slot.Hour, slot.Minute < 30 ? 0 : 30, 0));
                if (row == null)
                    continue;

                // Until service type is available on Appointment, put into General Consultation.
                var col = "GeneralConsultation";
                if (!row.CellsByServiceType.ContainsKey(col))
                    continue;

                var patient = await _appointmentService.GetPatientByIcAsync(appt.patient_IC);
                var isOnline = _appointmentService.IsAppUser(appt.patient_IC);
                var name = patient?.patient_name ?? appt.patient_IC;
                var label = isOnline ? $"{name} (Online)" : $"{name} (Walk-in)";

                row.CellsByServiceType[col] = new SlotCellDto
                {
                    IsBooked = true,
                    AppointmentId = appt.appointment_ID ?? string.Empty,
                    PatientIc = appt.patient_IC,
                    PatientName = label
                };
            }

            return new AppointmentScheduleGridDto
            {
                Date = day,
                ServiceTypes = serviceTypes,
                Rows = rows
            };
        }
    }
}
