using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class AppointmentScheduleService : IAppointmentScheduleService
    {
        public AppointmentScheduleService()
        {
        }

        public async Task<AppointmentScheduleGridDto> GetScheduleGridAsync(string doctorId, DateTime date)
        {
            await Task.Yield();

            // TODO: link with database
            // Required data to build schedule table:
            // 1) All supported service types for schedule columns ("type1", "type2", ...)
            // 2) All time slots for the given date (e.g., 09:00, 09:30, 10:00 ...)
            // 3) All appointments for doctorId on that date, including patient IC + service type + appointedAt
            // 4) Patient name lookup for each booked appointment
            //
            // Then map them into AppointmentScheduleGridDto:
            // - Grid.ServiceTypes = distinct service types
            // - Grid.Rows = one row per slot
            // - Each row has CellsByServiceType[serviceType] = booked/unbooked cell

            return new AppointmentScheduleGridDto
            {
                Date = date.Date,
                ServiceTypes = new List<string>(),
                Rows = new List<TimeSlotRowDto>()
            };
        }
    }
}
