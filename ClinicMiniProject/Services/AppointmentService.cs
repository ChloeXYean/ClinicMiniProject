using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class AppointmentService : IAppointmentService
    {
        // TODO: link with database (replace in-memory lists with real DB queries)
        private static readonly List<Appointment> _appointments = new();
        private static readonly List<Patient> _patients = new();

        private static readonly Random _rng = new();

        private readonly List<Appointment> appointments;
        private readonly List<DocAvailable> availabilityList;

        public AppointmentService()
        {
            appointments = _appointments;
            availabilityList = new List<DocAvailable>();
        }
        public AppointmentService(List<Appointment> apts, List<DocAvailable> avails)
        {
            appointments = apts;
            availabilityList = avails;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffAndDateRangeAsync(
            string staffId, 
            DateTime startDate, 
            DateTime endDate)
        {
            await Task.Yield();
            return _appointments
                .Where(a => a.staff_ID == staffId && a.appointedAt >= startDate && a.appointedAt < endDate)
                .ToList();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string staffId)
        {
            await Task.Yield();
            var now = DateTime.Now;
            return _appointments
                .Where(a => a.staff_ID == staffId && a.appointedAt > now && (a.status == "Scheduled" || a.status == "Pending"))
                .OrderBy(a => a.appointedAt)
                .ToList();
        }

        public bool IsAppUser(string patientId)
        {
            // TODO: link with database
            return _patients.Any(p => p.patient_IC == patientId && p.isAppUser);
        }

        public async Task<Patient?> GetPatientByIcAsync(string patientIc)
        {
            // TODO: link with database
            await Task.Yield();
            return _patients.FirstOrDefault(p => p.patient_IC == patientIc);
        }

        public async Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string nameQuery)
        {
            // TODO: link with database
            await Task.Yield();
            if (string.IsNullOrWhiteSpace(nameQuery))
                return Enumerable.Empty<Patient>();

            var q = nameQuery.Trim();
            return _patients
                .Where(p => !string.IsNullOrWhiteSpace(p.patient_name) && p.patient_name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
        {
            // TODO: link with database
            await Task.Yield();
            return _appointments.FirstOrDefault(a => a.appointment_ID == appointmentId);
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            // TODO: link with database
            await Task.Yield();
            var idx = _appointments.FindIndex(a => a.appointment_ID == appointment.appointment_ID);
            if (idx >= 0)
            {
                _appointments[idx] = appointment;
                return;
            }

            _appointments.Add(appointment);
        }

        public async Task<Patient?> GetRandomWalkInPatientAsync()
        {
            // Walk-in patient = not an app user
            // TODO: link with database
            await Task.Yield();
            var walkIns = _patients.Where(p => !p.isAppUser).ToList();
            if (walkIns.Count == 0)
                return null;

            return walkIns[_rng.Next(walkIns.Count)];
        }

        public DateTime? AssignWalkInTimeSlot(string doctorId, DateTime preferredDate, int workStartHour = 9, int workEndHour = 17, TimeSpan slotDuration = default)
        {
            if (slotDuration == default) slotDuration = TimeSpan.FromHours(1);

            var availability = availabilityList
                .FirstOrDefault(a => a.staff_ID == doctorId && a.IsAvailable(preferredDate.DayOfWeek));

            if (availability == null)
                return null;

            var bookedSlots = appointments
                .Where(a => a.staff_ID == doctorId && a.appointedAt.HasValue && a.appointedAt.Value.Date == preferredDate.Date)
                .Select(a => a.appointedAt!.Value)
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

        public void AddAppointment(Appointment appt)
        {
            appointments.Add(appt);
        }
    }

}
