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
            var startHour = 10;
            var endHour = 22;

            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, day, endOfDay))
                            ?.Where(a => a.appointedAt.HasValue)
                            .ToList() ?? new();

            var serviceTypes = new List<string>
            {
                "GeneralConsultation", "FollowUpTreatment", "TestResultDiscussion", "VaccinationInjection", "MedicalScreening"
            };

            var start = day.AddHours(startHour);
            var rows = new List<TimeSlotRowDto>();

            for (var t = start; t < day.AddHours(endHour); t = t.AddMinutes(30))
            {
                var row = new TimeSlotRowDto { SlotStart = t };
                foreach (var st in serviceTypes)
                    row.CellsByServiceType[st] = new SlotCellDto { IsBooked = false };
                rows.Add(row);
            }

            foreach (var appt in appts)
            {
                var slot = appt.appointedAt!.Value;

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
                        // Use loaded data (No DB call)
                        var patient = appt.Patient;
                        var isOnline = patient?.isAppUser ?? false;
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
            // Map database service_type values to grid column names
            return serviceType switch
            {
                "General Consultation" => "GeneralConsultation",
                "Follow Up Treatment" => "FollowUpTreatment",
                "Test Result Discussion" => "TestResultDiscussion",
                "Vaccination/Injection" => "VaccinationInjection",
                "Medical Checkup" => "MedicalScreening",
                "Medical Screening" => "MedicalScreening",
                _ => "GeneralConsultation" // Default fallback
            };
        }
    }
}
