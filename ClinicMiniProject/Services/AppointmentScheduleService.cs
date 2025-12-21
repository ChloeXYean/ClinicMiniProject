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

            System.Diagnostics.Debug.WriteLine($"[Schedule] Loading for Doc: {doctorId} on {day:dd/MM/yyyy}...");

            // 1. Fetch appointments (Fast SQL Query - 1 Call Only)
            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, day, endOfDay))
                ?.Where(a => a.appointedAt.HasValue)
                .ToList() ?? new();

            System.Diagnostics.Debug.WriteLine($"[Schedule] Found {appts.Count} appointments in Database.");

            // 2. Setup Grid Structure (Memory only, very fast)
            var serviceTypes = new List<string>
            {
                "GeneralConsultation", "FollowUpTreatment", "TestResultDiscussion", "VaccinationInjection", "MedicalScreening"
            };

            var startHour = 9;
            var endHour = 21;
            var start = day.AddHours(startHour);
            var rows = new List<TimeSlotRowDto>();

            // Generate 24 slots (30 mins each)
            for (var t = start; t < day.AddHours(endHour); t = t.AddMinutes(30))
            {
                var row = new TimeSlotRowDto { SlotStart = t };
                foreach (var st in serviceTypes)
                    row.CellsByServiceType[st] = new SlotCellDto { IsBooked = false };
                rows.Add(row);
            }

            // 3. Fill Data using Math (No Database calls in loop)
            foreach (var appt in appts)
            {
                var slot = appt.appointedAt!.Value;

                // Calculate which row index this appointment belongs to
                // e.g. 10:00 AM - 9:00 AM = 60 mins -> Index 2
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
                        // Use the data already loaded in 'appt.Patient'
                        var patient = appt.Patient;
                        var isOnline = patient?.isAppUser ?? false;
                        var name = patient?.patient_name ?? appt.patient_IC;

                        var label = isOnline ? $"{name} (App)" : $"{name} (Walk-in)";

                        row.CellsByServiceType[col] = new SlotCellDto
                        {
                            IsBooked = true,
                            AppointmentId = appt.appointment_ID ?? string.Empty,
                            PatientIc = appt.patient_IC,
                            PatientName = label
                        };

                        System.Diagnostics.Debug.WriteLine($"[Schedule] Mapped {name} to {slot:HH:mm} - {col}");
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