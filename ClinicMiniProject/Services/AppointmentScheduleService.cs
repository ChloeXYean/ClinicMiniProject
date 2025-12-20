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
            System.Diagnostics.Debug.WriteLine("=== GetScheduleGridAsync START ===");
            System.Diagnostics.Debug.WriteLine($"Doctor ID: '{doctorId}'");
            System.Diagnostics.Debug.WriteLine($"Date: {date:yyyy-MM-dd}");
            
            var day = date.Date;
            var endOfDay = day.AddDays(1);

            // Service columns required by UI.
            var serviceTypes = new List<string>
            {
                "GeneralConsultation",
                "FollowUpTreatment",
                "TestResultDiscussion",
                "VaccinationInjection",
                "MedicalScreening"
            };

            System.Diagnostics.Debug.WriteLine($"Fetching appointments for doctor '{doctorId}' from {day:yyyy-MM-dd HH:mm} to {endOfDay:yyyy-MM-dd HH:mm}");
            
            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, day, endOfDay))
                ?.Where(a => a.appointedAt.HasValue)
                .ToList() ?? new();

            System.Diagnostics.Debug.WriteLine($"Found {appts.Count} appointments");
            
            foreach (var appt in appts)
            {
                System.Diagnostics.Debug.WriteLine($"  - Appt ID: {appt.appointment_ID}, Patient: {appt.patient_IC}, Time: {appt.appointedAt:yyyy-MM-dd HH:mm}, Service: {appt.service_type}, Status: {appt.status}");
            }

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

                // Find matching row (round to 30-min slot)
                var slotMinute = slot.Minute < 30 ? 0 : 30;
                var row = rows.FirstOrDefault(r => r.SlotStart == new DateTime(slot.Year, slot.Month, slot.Day, slot.Hour, slotMinute, 0));
                if (row == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No matching time slot found for appointment at {slot}");
                    continue;
                }

                // Map service_type from database to column name
                var serviceType = appt.service_type ?? "General Consultation";
                var col = MapServiceTypeToColumn(serviceType);
                
                System.Diagnostics.Debug.WriteLine($"Appointment: {appt.appointment_ID}, Service: {serviceType} → Column: {col}, Time: {slot:HH:mm}");
                
                if (!row.CellsByServiceType.ContainsKey(col))
                {
                    System.Diagnostics.Debug.WriteLine($"Column {col} not found in grid");
                    continue;
                }

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
                
                System.Diagnostics.Debug.WriteLine($"Assigned patient {name} to slot {slot:HH:mm} in column {col}");
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
