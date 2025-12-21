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

            // 1. Fetch appointments ONE TIME (Fast)
            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, day, endOfDay))
                ?.Where(a => a.appointedAt.HasValue)
                .ToList() ?? new();

            // 2. Setup Grid
            var serviceTypes = new List<string>
            {
                "GeneralConsultation", "FollowUpTreatment", "TestResultDiscussion", "VaccinationInjection", "MedicalScreening"
            };

            var start = day.AddHours(9);
            var end = day.AddHours(21);
            var rows = new List<TimeSlotRowDto>();

            // Create 24 slots (30 mins each)
            for (var t = start; t < end; t = t.AddMinutes(30))
            {
                var row = new TimeSlotRowDto { SlotStart = t };
                foreach (var st in serviceTypes)
                    row.CellsByServiceType[st] = new SlotCellDto { IsBooked = false };
                rows.Add(row);
            }

            // 3. Fill Data (Memory Only - Instant)
            foreach (var appt in appts)
            {
                var slot = appt.appointedAt!.Value;

                // Calculate Row Index using Math (No looping needed)
                var totalMinutes = (slot - start).TotalMinutes;
                if (totalMinutes < 0) continue;
                int index = (int)(totalMinutes / 30);

                if (index >= 0 && index < rows.Count)
                {
                    var row = rows[index];
                    var serviceType = appt.service_type ?? "General Consultation";
                    var col = MapServiceTypeToColumn(serviceType);

                    if (row.CellsByServiceType.ContainsKey(col))
                    {
                        // FIX: Use appt.Patient directly (Avoids DB call)
                        var patient = appt.Patient;
                        var isOnline = patient?.isAppUser ?? false;
                        var name = patient?.patient_name ?? appt.patient_IC;
                        var label = isOnline ? $"{name} (Online)" : $"{name} (Walk-in)";

                        row.CellsByServiceType[col] = new SlotCellDto
                        {
                            IsBooked = true, // This triggers your Converter color
                            AppointmentId = appt.appointment_ID ?? string.Empty,
                            PatientIc = appt.patient_IC,
                            PatientName = label
                        };
                    }
                }
            }

            return new AppointmentScheduleGridDto
            {
                Date = day,
                ServiceTypes = serviceTypes,
                Rows = rows
            };
        }

        private string MapServiceTypeToColumn(string serviceType)
        {
            return serviceType switch
            {
                "General Consultation" => "GeneralConsultation",
                "Follow Up Treatment" => "FollowUpTreatment",
                "Test Result Discussion" => "TestResultDiscussion",
                "Vaccination/Injection" => "VaccinationInjection",
                "Medical Checkup" => "MedicalScreening",
                "Medical Screening" => "MedicalScreening",
                _ => "GeneralConsultation"
            };
        }
    }
}