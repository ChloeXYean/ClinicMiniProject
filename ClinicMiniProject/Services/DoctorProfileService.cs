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

        public DoctorProfileService(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<DoctorProfileDto?> GetDoctorProfileAsync(string doctorId)
        {
            await Task.Yield();
            
            System.Diagnostics.Debug.WriteLine($"=== GetDoctorProfileAsync Started for ID: {doctorId} ===");

            // NOTE: Current project has no Staff repository/db service. AuthService keeps staff in-memory.
            // TODO: link with database (load staff profile by doctorId)
            var current = _authService.GetCurrentUser();
            System.Diagnostics.Debug.WriteLine($"AuthService current user: {current?.staff_ID}, Name: {current?.staff_name}, Contact: {current?.staff_contact}");
            
            Staff? staff = current != null && current.staff_ID == doctorId ? current : null;
            System.Diagnostics.Debug.WriteLine($"Staff match result: {staff != null}");

            if (staff == null)
            {
                System.Diagnostics.Debug.WriteLine("No staff found - returning null");
                return null;
            }

            var extras = GetOrCreateExtras(doctorId, staff);
            System.Diagnostics.Debug.WriteLine($"Extras - WorkingHours: {extras.WorkingHoursText}, Services: {extras.ServicesProvided.Count}");

            var result = new DoctorProfileDto
            {
                DoctorId = staff.staff_ID,
                Name = staff.staff_name,
                PhoneNo = staff.staff_contact,
                WorkingHoursText = extras.WorkingHoursText,
                ServicesProvided = extras.ServicesProvided.ToList(),
                ProfileImageUri = extras.ProfileImageUri
            };
            
            System.Diagnostics.Debug.WriteLine($"Returning DoctorProfileDto - ID: {result.DoctorId}, Name: {result.Name}, Phone: {result.PhoneNo}");
            System.Diagnostics.Debug.WriteLine($"=== GetDoctorProfileAsync Completed ===");
            
            return result;
        }

        public async Task UpdateDoctorProfileAsync(string doctorId, DoctorProfileUpdateDto update)
        {
            var current = _authService.GetCurrentUser();
            if (current == null || current.staff_ID != doctorId)
                throw new InvalidOperationException("Not authorized to update this profile.");

            // Update fields that exist in Staff model
            bool hasChanges = false;
            
            if (!string.IsNullOrWhiteSpace(update.Name) && current.staff_name != update.Name)
            {
                current.staff_name = update.Name;
                hasChanges = true;
                System.Diagnostics.Debug.WriteLine($"Updated staff_name to: {update.Name}");
            }

            if (!string.IsNullOrWhiteSpace(update.PhoneNo) && current.staff_contact != update.PhoneNo)
            {
                current.staff_contact = update.PhoneNo;
                hasChanges = true;
                System.Diagnostics.Debug.WriteLine($"Updated staff_contact to: {update.PhoneNo}");
            }

            // Save to database if there are changes
            if (hasChanges)
            {
                try
                {
                    var staffInDb = await _context.Staffs.FindAsync(doctorId);
                    if (staffInDb != null)
                    {
                        staffInDb.staff_name = current.staff_name;
                        staffInDb.staff_contact = current.staff_contact;
                        
                        await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"Successfully saved changes to database for doctor: {doctorId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Staff {doctorId} not found in database");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving to database: {ex.Message}");
                    throw new InvalidOperationException($"Failed to save profile changes: {ex.Message}");
                }
            }

            // Store services in extras dictionary (in-memory for now)
            var extras = GetOrCreateExtras(doctorId, current);

            if (!string.IsNullOrWhiteSpace(update.WorkingHoursText))
            {
                extras.WorkingHoursText = update.WorkingHoursText;
                System.Diagnostics.Debug.WriteLine($"Updated working hours to: {update.WorkingHoursText}");
            }

            if (update.ServicesProvided != null)
            {
                extras.ServicesProvided = update.ServicesProvided.ToList();
                System.Diagnostics.Debug.WriteLine($"Updated services count: {extras.ServicesProvided.Count}");
            }

            if (!string.IsNullOrWhiteSpace(update.ProfileImageUri))
            {
                extras.ProfileImageUri = update.ProfileImageUri;
                System.Diagnostics.Debug.WriteLine($"Updated profile image to: {update.ProfileImageUri}");
            }

            System.Diagnostics.Debug.WriteLine("Profile update completed successfully");
        }

        private static ProfileExtras GetOrCreateExtras(string doctorId, Staff staff)
        {
            if (_extrasByDoctorId.TryGetValue(doctorId, out var extras))
                return extras;

            // Initialize with default services since Staff model doesn't have specialities field yet
            extras = new ProfileExtras
            {
                WorkingHoursText = "9:00 AM - 9:00 PM",
                ProfileImageUri = "dotnet_bot.png",
                ServicesProvided = new List<string> 
                { 
                    "General Consultation",
                    "Health Checkup",
                    "Medical Screening"
                }
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
