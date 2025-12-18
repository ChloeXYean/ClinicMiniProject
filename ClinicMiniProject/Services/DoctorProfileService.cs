using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly IAuthService _authService;

        // TODO: link with database
        // Store profile-only fields that are not present in current Staff model:
        // - working hours
        // - profile image
        // - services provided (separate list)
        private static readonly Dictionary<string, ProfileExtras> _extrasByDoctorId = new();

        private sealed class ProfileExtras
        {
            public string WorkingHoursText { get; set; } = "9.00AM - 9.00PM";
            public List<string> ServicesProvided { get; set; } = new();
            public string ProfileImageUri { get; set; } = string.Empty;
        }

        public DoctorProfileService(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<DoctorProfileDto?> GetDoctorProfileAsync(string doctorId)
        {
            await Task.Yield();

            // NOTE: Current project has no Staff repository/db service. AuthService keeps staff in-memory.
            // TODO: link with database (load staff profile by doctorId)
            var current = _authService.GetCurrentUser();
            Staff? staff = current != null && current.staff_ID == doctorId ? current : null;

            if (staff == null)
                return null;

            var extras = GetOrCreateExtras(doctorId, staff);

            return new DoctorProfileDto
            {
                DoctorId = staff.staff_ID,
                Name = staff.staff_name,
                PhoneNo = staff.staff_contact,
                WorkingHoursText = extras.WorkingHoursText,
                ServicesProvided = extras.ServicesProvided.ToList(),
                ProfileImageUri = extras.ProfileImageUri
            };
        }

        public async Task UpdateDoctorProfileAsync(string doctorId, DoctorProfileUpdateDto update)
        {
            await Task.Yield();

            var current = _authService.GetCurrentUser();
            if (current == null || current.staff_ID != doctorId)
                throw new InvalidOperationException("Not authorized to update this profile.");

            // Update fields that exist in Staff model
            if (!string.IsNullOrWhiteSpace(update.Name))
                current.staff_name = update.Name;

            if (!string.IsNullOrWhiteSpace(update.PhoneNo))
                current.staff_contact = update.PhoneNo;

            // Specialists/Services tags: persist into Staff.specialities as comma-separated values
            // so patient booking can search doctor by specialists.
            // TODO: link with database
            // For better search/indexing, normalize into a separate table (e.g., DoctorSpecialty(DoctorId, SpecialtyName)).
            if (update.ServicesProvided != null)
            {
                var tags = update.ServicesProvided
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                current.specialities = string.Join(", ", tags);
            }

            var extras = GetOrCreateExtras(doctorId, current);

            if (!string.IsNullOrWhiteSpace(update.WorkingHoursText))
                extras.WorkingHoursText = update.WorkingHoursText;

            if (update.ServicesProvided != null)
                extras.ServicesProvided = ParseServices(current.specialities);

            if (!string.IsNullOrWhiteSpace(update.ProfileImageUri))
                extras.ProfileImageUri = update.ProfileImageUri;

            // TODO: link with database
            // Persist:
            // - Staff name/contact
            // - profile image
            // - working hours
            // - services provided
        }

        private static ProfileExtras GetOrCreateExtras(string doctorId, Staff staff)
        {
            if (_extrasByDoctorId.TryGetValue(doctorId, out var extras))
                return extras;

            extras = new ProfileExtras
            {
                WorkingHoursText = "9.00AM - 9.00PM",
                ProfileImageUri = string.Empty,
                ServicesProvided = ParseServices(staff.specialities)
            };

            _extrasByDoctorId[doctorId] = extras;
            return extras;
        }

        private static List<string> ParseServices(string? specialities)
        {
            if (string.IsNullOrWhiteSpace(specialities))
                return new List<string>();

            // Allow comma-separated specialities
            return specialities
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
    }
}
