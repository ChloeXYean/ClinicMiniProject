using Microsoft.Maui.Graphics; 
namespace ClinicMiniProject.Dtos
{
    public class AppointmentHistoryItem
    {
        public string Time { get; set; }
        public string Date { get; set; }
        public string Details { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }

        // UI-Specific Properties
        public Color StatusColor { get; set; }
        public Color BadgeColor { get; set; }
        public Color CardBackgroundColor { get; set; }
    }
}