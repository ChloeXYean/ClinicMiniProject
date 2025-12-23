using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicMiniProject.Services
{
    public class NurseProfileService : INurseProfileService
    {
        private readonly AppDbContext _context;

        private static readonly Dictionary<string, NurseProfileExtras> _extrasByNurseId = new();
        private sealed class NurseProfileExtras
        {
            public string Department { get; set; } = "General Nursing";
            public string ProfileImageUri { get; set; } = "profilepicture.png";
        }
        public NurseProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NurseProfileDto?> GetNurseProfileAsync(string nurseId)
        {
            var nurse = await _context.Staffs.FirstOrDefaultAsync(s => s.staff_ID == nurseId && !s.isDoctor);

            if (nurse == null) return null;

            var extras = GetOrCreateExtras(nurseId);

            return new NurseProfileDto
            {
                NurseId = nurse.staff_ID,
                Name = nurse.staff_name,
                PhoneNo = nurse.staff_contact ?? string.Empty,
                ICNumber = string.Empty,
                Department = extras.Department,
                ProfileImageUri = extras.ProfileImageUri
            };
        }
        public async Task<bool> UpdateNurseProfileAsync(string nurseId, NurseProfileUpdateDto update)
        {
            try
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.staff_ID == nurseId);
                if (staff == null) return false;

                bool dbChanged = false;
                if (staff.staff_name != update.Name) { staff.staff_name = update.Name; dbChanged = true; }
                if (staff.staff_contact != update.PhoneNo) { staff.staff_contact = update.PhoneNo; dbChanged = true; }

                if (dbChanged) await _context.SaveChangesAsync();

                var extras = GetOrCreateExtras(nurseId);
                extras.Department = update.Department;
                if (!string.IsNullOrEmpty(update.ProfileImageUri)) extras.ProfileImageUri = update.ProfileImageUri;

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profile: {ex.Message}");
                return false;
            }
        }
        private NurseProfileExtras GetOrCreateExtras(string nurseId)
        {
            if (!_extrasByNurseId.ContainsKey(nurseId))
            {
                _extrasByNurseId[nurseId] = new NurseProfileExtras();
            }
            return _extrasByNurseId[nurseId];
        }
    }
}