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
        public async Task AddAppointmentAsync(Appointment appt)
        {
            if (appt == null) return;

            if (string.IsNullOrEmpty(appt.appointment_ID))
                appt.appointment_ID = "A" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string staffId)
        {
            var now = DateTime.Now;

            return await _context.Appointments
                .Where(a => a.staff_ID == staffId &&
                            a.appointedAt.HasValue &&
                            a.appointedAt > now &&
                            (a.status == "Pending"))
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

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIcAsync(string patientIc)
        {
            return await _context.Appointments
                .Include(a => a.Staff)
                .Where(a => a.patient_IC == patientIc)
                .OrderByDescending(a => a.appointedAt)
                .ToListAsync();
        }
    }
}
