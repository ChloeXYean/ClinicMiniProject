//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using ClinicMiniProject.Models;
//using ClinicMiniProject.Services.Interfaces;

//namespace ClinicMiniProject.Services
//{
//    public class AppointmentService : IAppointmentService
//    {
//        private static readonly List<Appointment> _appointments = new();
//        private static readonly List<Patient> _patients = new();

//        private static readonly Random _rng = new();

//        private readonly List<Appointment> appointments;
//        private readonly List<DocAvailable> availabilityList;
//        private readonly AppDbContext _context;

//        public AppointmentService(AppDbContext context)
//        {
//            appointments = _appointments;
//            availabilityList = new List<DocAvailable>();
//            _context = context;
//        }
//        public AppointmentService(List<Appointment> apts, List<DocAvailable> avails)
//        {
//            appointments = apts;
//            availabilityList = avails;
//        }

//        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffAndDateRangeAsync(string staffId, DateTime startDate, DateTime endDate)
//        {
//            await _context.SaveChangesAsync();
//            return new List<Appointment>();
//        }

//        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string staffId)
//        {
//            await Task.Yield();
//            var now = DateTime.Now;
//            return _appointments
//                .Where(a => a.staff_ID == staffId && a.appointedAt > now && (a.status == "Scheduled" || a.status == "Pending"))
//                .OrderBy(a => a.appointedAt)
//                .ToList();
//        }

//        public bool IsAppUser(string patientId)
//        {
//            // TODO: link with database
//            return _patients.Any(p => p.patient_IC == patientId && p.isAppUser);
//        }

//        public async Task<Patient?> GetPatientByIcAsync(string patientIc)
//        {
//            // TODO: link with database
//            await Task.Yield();
//            return _patients.FirstOrDefault(p => p.patient_IC == patientIc);
//        }

//        public async Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string nameQuery)
//        {
//            // TODO: link with database
//            await Task.Yield();
//            if (string.IsNullOrWhiteSpace(nameQuery))
//                return Enumerable.Empty<Patient>();

//            var q = nameQuery.Trim();
//            return _patients
//                .Where(p => !string.IsNullOrWhiteSpace(p.patient_name) && p.patient_name.Contains(q, StringComparison.OrdinalIgnoreCase))
//                .ToList();
//        }

//        public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
//        {
//            // TODO: link with database
//            await Task.Yield();
//            return _appointments.FirstOrDefault(a => a.appointment_ID == appointmentId);
//        }

//        public async Task UpdateAppointmentAsync(Appointment appointment)
//        {
//            // TODO: link with database
//            await Task.Yield();
//            var idx = _appointments.FindIndex(a => a.appointment_ID == appointment.appointment_ID);
//            if (idx >= 0)
//            {
//                _appointments[idx] = appointment;
//                return;
//            }

//            _appointments.Add(appointment);
//        }

//        public async Task<Patient?> GetRandomWalkInPatientAsync()
//        {
//            // Walk-in patient = not an app user
//            // TODO: link with database
//            await Task.Yield();
//            var walkIns = _patients.Where(p => !p.isAppUser).ToList();
//            if (walkIns.Count == 0)
//                return null;

//            return walkIns[_rng.Next(walkIns.Count)];
//        }

//        public DateTime? AssignWalkInTimeSlot(string doctorId, DateTime preferredDate, int workStartHour = 9, int workEndHour = 17, TimeSpan slotDuration = default)
//        {
//            if (slotDuration == default) slotDuration = TimeSpan.FromHours(1);

//            var availability = availabilityList
//                .FirstOrDefault(a => a.staff_ID == doctorId && a.IsAvailable(preferredDate.DayOfWeek));

//            if (availability == null)
//                return null;

//            var bookedSlots = appointments
//                .Where(a => a.staff_ID == doctorId && a.appointedAt.HasValue && a.appointedAt.Value.Date == preferredDate.Date)
//                .Select(a => a.appointedAt!.Value)
//                .ToList();

//            var dayStart = new DateTime(preferredDate.Year, preferredDate.Month, preferredDate.Day, workStartHour, 0, 0);
//            var dayEndExclusive = new DateTime(preferredDate.Year, preferredDate.Month, preferredDate.Day, workEndHour, 0, 0);

//            var available = new List<DateTime>();
//            var slot = dayStart;
//            while (slot + slotDuration <= dayEndExclusive)
//            {
//                available.Add(slot);
//                slot = slot.Add(slotDuration);
//            }


//            foreach (var a in available)
//            {
//                bool overlaps = bookedSlots.Any(booked =>
//                {
//                    var bookedStart = booked;
//                    var bookedEnd = bookedStart.Add(slotDuration);
//                    var availableStartTime = a;
//                    var availableEndTime = availableStartTime.Add(slotDuration);

//                    return !(availableEndTime <= bookedStart || availableEndTime >= bookedEnd);
//                });

//                if (!overlaps)
//                    return a; // earliest free slot
//            }

//            return null;
//        }

//        public void AddAppointment(Appointment appt)
//        {
//            appointments.Add(appt);
//        }
//    }

//}
using ClinicMiniProject.Models;
using ClinicMiniProject.Repository;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicMiniProject.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _context;
        private readonly IAppointmentRepository _repo;

        public AppointmentService(AppDbContext context, IAppointmentRepository repo)
        {
            _context = context;
            _repo = repo;
        }
        public void AddAppointment(Appointment appt)
        {
            if (appt == null) return;

            if (string.IsNullOrEmpty(appt.appointment_ID))
                appt.appointment_ID = Guid.NewGuid().ToString()[..8];

            _context.Appointments.Add(appt);
            _context.SaveChanges();
        }
        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string staffId)
        {
            var now = DateTime.Now;

            return await _context.Appointments
                .Where(a => a.staff_ID == staffId &&
                            a.appointedAt.HasValue &&
                            a.appointedAt > now &&
                            (a.status == "Scheduled" || a.status == "Pending"))
                .OrderBy(a => a.appointedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffAndDateRangeAsync(
            string? staffId, DateTime startDate, DateTime endDate)
        {
            var query = _context.Appointments
                    .Where(a => a.appointedAt >= startDate && a.appointedAt <= endDate);

            if (!string.IsNullOrEmpty(staffId))
            {
                query = query.Where(a => a.staff_ID == staffId);
            }

            return await query.OrderBy(a => a.appointedAt).ToListAsync();
        }

        public DateTime? AssignWalkInTimeSlot(string doctorId,DateTime preferredDate,int workStartHour = 9,int workEndHour = 17,TimeSpan slotDuration = default)
        {
            if (slotDuration == default)
                slotDuration = TimeSpan.FromHours(1);

            var availability = _context.DocAvailables.FirstOrDefault(a => a.staff_ID == doctorId);

            if (availability == null || !availability.IsAvailable(preferredDate.DayOfWeek))
                return null;

            var bookedSlots = _context.Appointments.Where(a => a.staff_ID == doctorId &&a.appointedAt.HasValue &&a.appointedAt.Value.Date == preferredDate.Date)
                .Select(a => a.appointedAt!.Value)
                .ToList();

            var dayStart = preferredDate.Date.AddHours(workStartHour);
            var dayEnd = preferredDate.Date.AddHours(workEndHour);

            for (var slot = dayStart; slot.Add(slotDuration) <= dayEnd; slot = slot.Add(slotDuration))
            {
                bool overlaps = bookedSlots.Any(booked =>
                    slot < booked.Add(slotDuration) &&
                    slot.Add(slotDuration) > booked);

                if (!overlaps)
                    return slot;
            }

            return null;
        }

        public bool IsAppUser(string patientId)
        {
            var walkIns = _context.Patients.Where(p => !p.isAppUser).ToList();
            if (walkIns.Count != 0)
                return true;

            return false;
        }

        public Task<Patient?> GetPatientByIcAsync(string patientIc)
        {
            return _context.Patients.FirstOrDefaultAsync(p => p.patient_IC == patientIc);
        }

        public Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string nameQuery)
        {
            if (string.IsNullOrWhiteSpace(nameQuery))
                return Task.FromResult(Enumerable.Empty<Patient>());

            return Task.FromResult<IEnumerable<Patient>>(_context.Patients
                    .Where(p => p.patient_name.Contains(nameQuery))
                    .ToList());
        }

        public Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
        {
            return _context.Appointments.FirstOrDefaultAsync(a => a.appointment_ID == appointmentId);
        }

        public Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            return _context.SaveChangesAsync();
        }

        public Task<Patient?> GetRandomWalkInPatientAsync()
        {
            return _context.Patients
                .Where(p => !p.isAppUser)
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            return await _repo.GetAppointmentsByDateAsync(date);
        }

        //public async Task<Patient?> GetRandomWalkInPatientAsync()
        //{
        //    return await (
        //        from p in _context.Patients
        //        join a in _context.Appointments
        //            on p.patient_IC equals a.patient_IC
        //        where !p.isAppUser
        //        orderby a.bookedAt
        //        select p).FirstOrDefaultAsync();
        //}


    }
}
