using System;
using System.Collections.Generic;

namespace ClinicMiniProject.Dtps
{
    public class AppointmentScheduleGridDto
    {
        public DateTime Date { get; set; }
        public List<string> ServiceTypes { get; set; } = new();
        public List<TimeSlotRowDto> Rows { get; set; } = new();
    }

    public class TimeSlotRowDto
    {
        public DateTime SlotStart { get; set; }
        // The UI needs this to be a Property (get; set;) to read it!
        public Dictionary<string, SlotCellDto> CellsByServiceType { get; set; } = new();
    }

    public class SlotCellDto
    {
        // These MUST be 'public' and have '{ get; set; }'
        public bool IsBooked { get; set; }
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientIc { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string CellColor { get; set; } = "#FFFFFF";
    }
}