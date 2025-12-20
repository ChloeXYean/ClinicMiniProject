using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class DoctorDashboardService : IDoctorDashboardService
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthService _authService;

        public DoctorDashboardService(
            IAppointmentService appointmentService,
            IAuthService authService)
        {
            _appointmentService = appointmentService;
            _authService = authService;
        }

        public async Task<DoctorDashboardDataDto> GetDashboardDataAsync(string doctorId)
        {
            var today = DateTime.Today;
            var now = DateTime.Now;
            var endOfDay = today.AddDays(1);

            // Get all relevant appointments
            var todayAppointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(
                doctorId, today, endOfDay);
                
            var upcomingAppointments = await _appointmentService.GetUpcomingAppointmentsAsync(doctorId);

            // Process today's stats
            var todayStats = CalculateTodayStats(todayAppointments, today, now);
            
            // Process upcoming schedule
            var upcomingSchedule = CalculateUpcomingSchedule(upcomingAppointments, now);

            return new DoctorDashboardDataDto
            {
                Greeting = $"Hi, Dr. {_authService.GetDoctorName(doctorId)}",
                CurrentDate = now,
                TodayStats = todayStats,
                UpcomingSchedule = upcomingSchedule
            };
        }

        private TodayStatsDto CalculateTodayStats(IEnumerable<Appointment> appointments, DateTime today, DateTime now)
        {
            var todayAppointments = appointments?.ToList() ?? new List<Appointment>();
            
            // Filter appointments that have been consulted (status = "Completed" or "Consulted")
            var consultedAppointments = todayAppointments
                .Where(a => a.appointedAt.HasValue && a.appointedAt.Value.Date == today.Date && 
                           (a.status == "Completed" || a.status == "Consulted"))
                .ToList();

            // Count online vs walk-in (this is a simplification - you might need to adjust based on your actual logic)
            var onlineConsulted = consultedAppointments
                .Count(a => a.service_type == "Online");

            return new TodayStatsDto
            {
                TotalConsulted = consultedAppointments.Count,
                OnlineConsulted = onlineConsulted,
                WalkInConsulted = consultedAppointments.Count - onlineConsulted
            };
        }

        private UpcomingScheduleDto CalculateUpcomingSchedule(IEnumerable<Appointment> appointments, DateTime now)
        {
            var upcomingAppointments = appointments?
                .Where(a => a.appointedAt > now && 
                           (a.status == "Pending"))
                .OrderBy(a => a.appointedAt)
                .ToList() ?? new List<Appointment>();

            // Count online vs walk-in (simplified)
            var onlinePending = upcomingAppointments
                .Count(a => a.service_type == "Online");

            return new UpcomingScheduleDto
            {
                PendingAppointments = upcomingAppointments.Count,
                OnlinePending = onlinePending,
                WalkInPending = upcomingAppointments.Count - onlinePending,
                NextAppointmentTime = upcomingAppointments.FirstOrDefault()?.appointedAt
            };
        }
    }
}