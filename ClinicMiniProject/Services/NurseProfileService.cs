using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicMiniProject.Services
{
    public class NurseProfileService : INurseProfileService
    {
        private readonly AppDbContext _context;

        public NurseProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NurseProfileDto?> GetNurseProfileAsync(string nurseId)
        {
            var nurse = await _context.Staffs.FirstOrDefaultAsync(s => s.staff_ID == nurseId && !s.isDoctor);
            if (nurse == null) return null;

            return new NurseProfileDto
            {
                NurseId = nurse.staff_ID,
                Name = nurse.staff_name,
                PhoneNo = nurse.staff_contact ?? string.Empty,
                WorkingHoursText = "9:00 AM - 9:00 PM",
                Department = "General Nursing",
                ProfileImageUri = string.Empty
            };
        }

        public async Task<bool> UpdateNurseProfileAsync(string nurseId, NurseProfileUpdateDto update)
        {
            var nurse = await _context.Staffs.FirstOrDefaultAsync(s => s.staff_ID == nurseId && !s.isDoctor);
            if (nurse == null) return false;

            // Update basic profile information
            if (!string.IsNullOrWhiteSpace(update.Name))
                nurse.staff_name = update.Name;
            
            if (!string.IsNullOrWhiteSpace(update.PhoneNo))
                nurse.staff_contact = update.PhoneNo;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
