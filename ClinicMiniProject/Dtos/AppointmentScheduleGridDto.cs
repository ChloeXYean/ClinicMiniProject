using System;
using System.Collections.Generic;

namespace ClinicMiniProject.Dtos { 
    public class AppointmentScheduleGridDto
    {
        public DateTime Date { get; set; }
        public List<string> ServiceTypes { get; set; } = new();
        public List<TimeSlotRowDto> Rows { get; set; } = new();
    }

    public class TimeSlotRowDto
    {
        public DateTime SlotStart { get; set; }
        public Dictionary<string, SlotCellDto> CellsByServiceType { get; set; } = new();

        // --- ADD THESE PROPERTIES BELOW ---
        public SlotCellDto GeneralConsultation => CellsByServiceType.ContainsKey("GeneralConsultation") ? CellsByServiceType["GeneralConsultation"] : new SlotCellDto();
        public SlotCellDto FollowUpTreatment => CellsByServiceType.ContainsKey("FollowUpTreatment") ? CellsByServiceType["FollowUpTreatment"] : new SlotCellDto();
        public SlotCellDto TestResultDiscussion => CellsByServiceType.ContainsKey("TestResultDiscussion") ? CellsByServiceType["TestResultDiscussion"] : new SlotCellDto();
        public SlotCellDto VaccinationInjection => CellsByServiceType.ContainsKey("VaccinationInjection") ? CellsByServiceType["VaccinationInjection"] : new SlotCellDto();
        public SlotCellDto MedicalScreening => CellsByServiceType.ContainsKey("MedicalScreening") ? CellsByServiceType["MedicalScreening"] : new SlotCellDto();
    }

    public class SlotCellDto
    {
        public bool IsBooked { get; set; }
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientIc { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string CellColor { get; set; } = "#FFFFFF";
    }
}