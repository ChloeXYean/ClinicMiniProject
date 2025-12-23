using System;

namespace ClinicMiniProject.Dtos
{
    public sealed class DoctorDashboardDataDto
    {
        public string Greeting { get; init; } = string.Empty;
        public DateTime CurrentDate { get; init; }
        public TodayStatsDto TodayStats { get; init; } = new();
        public UpcomingScheduleDto UpcomingSchedule { get; init; } = new();
    }

    public sealed class TodayStatsDto
    {
        public int TotalConsulted { get; init; }
        public int OnlineConsulted { get; init; }
        public int WalkInConsulted { get; init; }
    }

    public sealed class UpcomingScheduleDto
    {
        public int PendingAppointments { get; init; }
        public int OnlinePending { get; init; }
        public int WalkInPending { get; init; }
        public DateTime? NextAppointmentTime { get; init; }
        public NextAppointmentDetailsDto NextAppointmentDetails { get; init; }
    }

    public sealed class NextAppointmentDetailsDto
    {
        public string AppointmentId { get; init; }
        public string PatientName { get; init; }
        public string PatientIC { get; init; }
        public string ServiceType { get; init; }
        public DateTime AppointmentTime { get; init; }
        public string Status { get; init; }
        public bool IsCurrentTimeSlot { get; init; }
    }

    public sealed class CurrentConsultationDto
    {
        public string AppointmentId { get; init; }
        public string PatientName { get; init; }
        public string PatientIC { get; init; }
        public string ServiceType { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime OriginalAppointmentTime { get; init; }
    }

    public sealed class ConsultationAppointmentDto
    {
        public string AppointmentId { get; init; }
        public string PatientName { get; init; }
        public string PatientIC { get; init; }
        public string ServiceType { get; init; }
        public DateTime AppointmentTime { get; init; }
        public string Status { get; init; }
        public bool CanStartConsultation { get; init; }
        public bool CanStartEarly { get; init; }
        public string TimeIndicatorColor { get; init; }
        public string StatusColor { get; init; }
        public TimeSpan TimeFromNow { get; init; }
    }
}
