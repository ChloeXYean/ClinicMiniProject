using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IAppointmentScheduleService
    {
        Task<AppointmentScheduleGridDto> GetScheduleGridAsync(string doctorId, DateTime date);
    }

    public sealed class AppointmentScheduleGridDto
    {
        public DateTime Date { get; init; }
        public List<string> ServiceTypes { get; init; } = new();
        public List<TimeSlotRowDto> Rows { get; init; } = new();
    }

    public sealed class TimeSlotRowDto
    {
        public DateTime SlotStart { get; init; }
        public Dictionary<string, SlotCellDto> CellsByServiceType { get; init; } = new();
    }

    public sealed class SlotCellDto
    {
        public bool IsBooked { get; init; }
        public string AppointmentId { get; init; } = string.Empty;
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
    }
}
