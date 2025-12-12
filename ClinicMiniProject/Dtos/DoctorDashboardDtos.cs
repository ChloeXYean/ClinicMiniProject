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
    }
}
