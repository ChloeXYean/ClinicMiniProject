using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicMiniProject.Services
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, ProfileExtras> _extrasByDoctorId = new();

        private sealed class ProfileExtras
        {
            public string WorkingHoursText { get; set; } = "9:00 AM - 9:00 PM";
            public string ProfileImageUri { get; set; } = string.Empty;
        }

        public DoctorProfileService(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<DoctorProfileDto?> GetDoctorProfileAsync(string doctorId)
        {
            var current = _authService.GetCurrentUser();
            Staff? staff = current != null && current.staff_ID == doctorId ? current : await _context.Staffs.FindAsync(doctorId);

            if (staff == null) return null;

            // Fetch availability from database
            var availability = await _context.DocAvailables.FindAsync(doctorId);

            bool mon = true, tue = true, wed = true, thu = true, fri = true, sat = true, sun = false;

            if (availability != null)
            {
                mon = availability.Monday;
                tue = availability.Tuesday;
                wed = availability.Wednesday;
                thu = availability.Thursday;
                fri = availability.Friday;
                sat = availability.Saturday;
                sun = availability.Sunday;
            }

            var extras = GetOrCreateExtras(doctorId, staff);

            return new DoctorProfileDto
            {
                DoctorId = staff.staff_ID,
                Name = staff.staff_name,
                PhoneNo = staff.staff_contact,
                WorkingHoursText = extras.WorkingHoursText,
                ProfileImageUri = extras.ProfileImageUri,

                IsMon = mon,
                IsTue = tue,
                IsWed = wed,
                IsThu = thu,
                IsFri = fri,
                IsSat = sat,
                IsSun = sun
            };
        }

        public async Task UpdateDoctorProfileAsync(string doctorId, DoctorProfileUpdateDto update)
        {
            var current = _authService.GetCurrentUser();
            if (current == null || current.staff_ID != doctorId)
                throw new InvalidOperationException("Not authorized.");

            // --- 1. Update Staff Table (Name/Phone) ---
            bool staffChanged = false;
            var staffInDb = await _context.Staffs.FindAsync(doctorId);

            if (staffInDb != null)
            {
                if (staffInDb.staff_name != update.Name) { staffInDb.staff_name = update.Name; staffChanged = true; }
                if (staffInDb.staff_contact != update.PhoneNo) { staffInDb.staff_contact = update.PhoneNo; staffChanged = true; }

                if (current.staff_name != update.Name) current.staff_name = update.Name;
            }

            // --- 2. Update Availability Table ---
            var availability = await _context.DocAvailables.FindAsync(doctorId);
            if (availability == null)
            {
                availability = new DocAvailable { staff_ID = doctorId };
                _context.DocAvailables.Add(availability);
                staffChanged = true;
            }

            availability.Monday = update.IsMon;
            availability.Tuesday = update.IsTue;
            availability.Wednesday = update.IsWed;
            availability.Thursday = update.IsThu;
            availability.Friday = update.IsFri;
            availability.Saturday = update.IsSat;
            availability.Sunday = update.IsSun;

            if (staffChanged || true)
            {
                await _context.SaveChangesAsync();
            }

            // --- 3. Update Extras (Memory) ---
            var extras = GetOrCreateExtras(doctorId, staffInDb ?? current);
            extras.WorkingHoursText = update.WorkingHoursText;
            if (!string.IsNullOrEmpty(update.ProfileImageUri)) extras.ProfileImageUri = update.ProfileImageUri;
        }

        private static ProfileExtras GetOrCreateExtras(string doctorId, Staff staff)
        {
            if (_extrasByDoctorId.TryGetValue(doctorId, out var extras))
                return extras;

            extras = new ProfileExtras
            {
                WorkingHoursText = "9:00 AM - 9:00 PM",
                ProfileImageUri = "dotnet_bot.png"
            };

            _extrasByDoctorId[doctorId] = extras;
            return extras;
        }
    }
}