using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Services
{
    public class AppointmentService
    {
        private readonly List<Appointment> appointments;
        private readonly List<DoctorAvailability> availabilityList;

        public AppointmentService(List<Appointment> apts, List<DoctorAvailability> avails)
        {
            appointments = apts;
            availabilityList = avails;
        }

        //Temporary is 9am - 5pm
        public DateTime? AssignWalkInTimeSlot(string doctorId,DateTime preferredDate,int workStartHour = 9,int workEndHour = 17, TimeSpan slotDuration = default)
        {
            if (slotDuration == default) slotDuration = TimeSpan.FromHours(1);

            var availability = availabilityList
                .FirstOrDefault(a => a.staff_ID == doctorId && a.IsAvailable(preferredDate.DayOfWeek));

            if (availability == null)
                return null;

            var bookedSlots = appointments
                .Where(a => a.staff_ID == doctorId && a.appointedAt.Date == preferredDate.Date)
                .Select(a => a.appointedAt)
                .ToList();

            var dayStart = new DateTime(preferredDate.Year, preferredDate.Month, preferredDate.Day, workStartHour, 0, 0);
            var dayEndExclusive = new DateTime(preferredDate.Year, preferredDate.Month, preferredDate.Day, workEndHour, 0, 0);

            var available = new List<DateTime>();
            var slot = dayStart;
            while (slot + slotDuration <= dayEndExclusive)
            {
                available.Add(slot);
                slot = slot.Add(slotDuration);
            }


            foreach (var a in available)
            {
                bool overlaps = bookedSlots.Any(booked =>
                {
                    var bookedStart = booked;
                    var bookedEnd = bookedStart.Add(slotDuration);
                    var availableStartTime = a;
                    var availableEndTime = availableStartTime.Add(slotDuration);

                    return !(availableEndTime <= bookedStart || availableEndTime >= bookedEnd);
                });

                if (!overlaps)
                    return a; // earliest free slot
            }

            return null;
        }

    }

}
