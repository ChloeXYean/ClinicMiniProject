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
        public async Task<bool> AddAppointmentAsync(Appointment appt)
        {
            if (appt == null || !appt.appointedAt.HasValue)
                return false;

            // Check for conflicts before adding
            bool hasConflict = await HasConflictingAppointmentAsync(
                appt.patient_IC,
                appt.staff_ID,
                appt.appointedAt.Value);

            if (hasConflict)
                return false;

            if (string.IsNullOrEmpty(appt.appointment_ID))
            {
                // Ensure a unique ID by checking if it already exists
                string newId;
                bool exists;
                do
                {
                    newId = "A" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                    exists = await _context.Appointments.AnyAsync(a => a.appointment_ID == newId);
                } while (exists);

                appt.appointment_ID = newId;
            }

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasConflictingAppointmentAsync(string patientIc, string doctorId, DateTime appointmentTime)
        {
            // Check if patient already has an appointment at this time (any doctor)
            bool patientConflict = await _context.Appointments
                .AnyAsync(a => a.patient_IC == patientIc
                            && a.appointedAt == appointmentTime
                            && a.status != "Cancelled");

            if (patientConflict)
                return true;

            // Check if doctor already has an appointment at this time (any patient)
            bool doctorConflict = await _context.Appointments
                .AnyAsync(a => a.staff_ID == doctorId
                            && a.appointedAt == appointmentTime
                            && a.status != "Cancelled");

            return doctorConflict;
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
                    .Include(a => a.Patient)
                    .Include(a => a.Staff)
                    .Where(a => a.appointedAt >= startDate && a.appointedAt <= endDate);

            if (!string.IsNullOrEmpty(staffId))
            {
                query = query.Where(a => a.staff_ID == staffId);
            }

            return await query.OrderBy(a => a.appointedAt).ToListAsync();
        }

        public DateTime? AssignWalkInTimeSlot(string doctorId, DateTime preferredDate, int workStartHour = 9, int workEndHour = 21, TimeSpan slotDuration = default)
        {
            if (slotDuration == default)
                slotDuration = TimeSpan.FromHours(1);

            var availability = _context.DocAvailables.FirstOrDefault(a => a.staff_ID == doctorId);

            if (availability == null || !availability.IsAvailable(preferredDate.DayOfWeek))
                return null;

            var bookedSlots = _context.Appointments.Where(a => a.staff_ID == doctorId && a.appointedAt.HasValue && a.appointedAt.Value.Date == preferredDate.Date)
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
            //1223
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


        public async Task<IEnumerable<Appointment>> GetAppointmentHistoryForStaffAsync(string staffId)
        {
            return await _context.Appointments
                .Where(a => a.staff_ID == staffId && a.appointedAt < DateTime.Now)
                .Include(a => a.Patient) // Important: Load Patient info!
                .OrderByDescending(a => a.appointedAt)
                .ToListAsync();
        }

        // New methods for synchronized appointment booking
        public async Task<List<TimeSpan>> GetBookedTimeSlotsAsync(string doctorId, DateTime date)
        {
            var appointments = await _context.Appointments
                .Where(a => a.staff_ID == doctorId
                            && a.appointedAt.HasValue
                            && a.appointedAt.Value.Date == date.Date
                            && a.status != "Cancelled")
                .Select(a => a.appointedAt!.Value.TimeOfDay)
                .ToListAsync();

            return appointments;
        }

        public async Task<bool> PatientHasAppointmentAtTimeAsync(string patientIc, DateTime appointmentTime)
        {
            return await _context.Appointments
                .AnyAsync(a => a.patient_IC == patientIc
                            && a.appointedAt == appointmentTime
                            && a.status != "Cancelled");
        }

        public async Task<List<TimeSpan>> GetPatientBookedTimeSlotsForDateAsync(string patientIc, DateTime date)
        {
            var appointments = await _context.Appointments
                .Where(a => a.patient_IC == patientIc
                            && a.appointedAt.HasValue
                            && a.appointedAt.Value.Date == date.Date
                            && a.status != "Cancelled")
                .Select(a => a.appointedAt!.Value.TimeOfDay)
                .ToListAsync();

            return appointments;
        }

        public async Task<bool> IsAnyDoctorAvailableAtTimeAsync(DateTime appointmentDateTime)
        {
            // Get all doctors
            var doctors = await _context.Staffs
                .Include(s => s.Availability)
                .Where(s => s.isDoctor)
                .ToListAsync();

            var dayOfWeek = appointmentDateTime.DayOfWeek;

            // Check if at least one doctor is available
            foreach (var doctor in doctors)
            {
                // Skip if doctor has no availability info
                if (doctor.Availability == null) continue;

                // Skip if doctor is not available on this day of week
                if (!doctor.Availability.IsAvailable(dayOfWeek)) continue;

                // Check if this doctor has a conflicting appointment
                bool hasConflict = await _context.Appointments
                    .AnyAsync(a => a.staff_ID == doctor.staff_ID
                                && a.appointedAt == appointmentDateTime
                                && a.status != "Cancelled");

                // If no conflict, this doctor is available!
                if (!hasConflict)
                    return true;
            }

            // No doctors available at this time
            return false;
        }

        public async Task<List<TimeSpan>> GetUnavailableTimeSlotsForDateAsync(DateTime date)
        {
            // Get all doctors and their appointments for the day in a single query
            var doctors = await _context.Staffs
                .Include(s => s.Availability)
                .Where(s => s.isDoctor)
                .ToListAsync();

            var dayOfWeek = date.DayOfWeek;
            var dayStart = date.Date.AddHours(9);  // Start at 9 AM
            var dayEnd = date.Date.AddHours(17);   // End at 5 PM

            // Get all appointments for this date
            var allAppointments = await _context.Appointments
                .Where(a => a.appointedAt.HasValue
                            && a.appointedAt.Value.Date == date.Date
                            && a.status != "Cancelled")
                .Select(a => new { a.staff_ID, TimeSlot = a.appointedAt!.Value.TimeOfDay })
                .ToListAsync();

            // Create a dictionary of doctor appointments
            var doctorAppointments = allAppointments
                .GroupBy(a => a.staff_ID)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TimeSlot).ToHashSet());

            var unavailableSlots = new List<TimeSpan>();

            // Check each hour from 9 AM to 5 PM
            for (int hour = 9; hour < 21; hour++)
            {
                var timeSlot = new TimeSpan(hour, 0, 0);
                bool anyDoctorAvailable = false;

                foreach (var doctor in doctors)
                {
                    // Skip if doctor has no availability info
                    if (doctor.Availability == null) continue;

                    // Skip if doctor is not available on this day of week
                    if (!doctor.Availability.IsAvailable(dayOfWeek)) continue;

                    // Check if this doctor has an appointment at this time
                    bool hasDoctorAppointment = doctorAppointments.ContainsKey(doctor.staff_ID)
                                                && doctorAppointments[doctor.staff_ID].Contains(timeSlot);

                    // If this doctor is free, the slot is available
                    if (!hasDoctorAppointment)
                    {
                        anyDoctorAvailable = true;
                        break;
                    }
                }

                // If no doctor is available, mark this slot as unavailable
                if (!anyDoctorAvailable)
                {
                    unavailableSlots.Add(timeSlot);
                }
            }

            return unavailableSlots;

        }

        public async Task CheckAndMarkLateAppointmentsAsync()
        {
            try
            {
                var now = DateTime.Now;
                // "Late" means 15 minutes have passed since the appointment time
                var lateThreshold = now.AddMinutes(-15);

                // Find all PENDING appointments that are older than the threshold
                var lateAppointments = await _context.Appointments
                    .Where(a => a.status == "Pending"
                                && a.appointedAt.HasValue
                                && a.appointedAt.Value < lateThreshold)
                    .ToListAsync();

                if (lateAppointments.Any())
                {
                    foreach (var appt in lateAppointments)
                    {
                        // Mark as "Missed" or "Cancelled" automatically
                        appt.status = "Missed";
                        // Optional: Add a remark
                        appt.nurse_remark = "Auto-cancelled: Patient arrived >15 mins late.";
                    }

                    _context.Appointments.UpdateRange(lateAppointments);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"[System] Auto-cancelled {lateAppointments.Count} late appointments.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] Failed to auto-cancel late appointments: {ex.Message}");
            }
        }

        public async Task AutoCancelExpiredAppointmentsAsync()
        {
            try
            {
                var today = DateTime.Today;

                // Find all PENDING appointments where the appointment date is before today
                var expiredAppointments = await _context.Appointments
                    .Where(a => a.status == "Pending"
                                && a.appointedAt.HasValue
                                && a.appointedAt.Value.Date < today)
                    .ToListAsync();

                if (expiredAppointments.Any())
                {
                    foreach (var appt in expiredAppointments)
                    {
                        appt.status = "Cancelled";
                        appt.nurse_remark = "Auto-cancelled: Appointment date has passed.";
                    }

                    _context.Appointments.UpdateRange(expiredAppointments);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"[System] Auto-cancelled {expiredAppointments.Count} expired appointments.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] Failed to auto-cancel expired appointments: {ex.Message}");
            }
        }

        public async Task<bool> CancelAppointmentAsync(string appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.appointment_ID == appointmentId);

                if (appointment == null || appointment.status != "Pending")
                    return false;

                appointment.status = "Cancelled";
                appointment.nurse_remark = "Cancelled by patient.";

                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] Failed to cancel appointment: {ex.Message}");
                return false;
            }
        }
    }
}