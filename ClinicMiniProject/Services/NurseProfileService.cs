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


                Department = "General Nursing",

                ProfileImageUri = "profilepicture.png",


                ICNumber = string.Empty
            };
        }

        public async Task<bool> UpdateNurseProfileAsync(string nurseId, NurseProfileUpdateDto update)
        {
            try
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.staff_ID == nurseId);

                if (staff == null) return false;

                staff.staff_name = update.Name;
                staff.staff_contact = update.PhoneNo;


                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profile: {ex.Message}");
                return false;
            }
        }
    }
}