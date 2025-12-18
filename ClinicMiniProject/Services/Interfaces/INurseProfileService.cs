using ClinicMiniProject.Models;
using ClinicMiniProject.Dtos;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface INurseProfileService
    {
        Task<NurseProfileDto?> GetNurseProfileAsync(string nurseId);
        Task<bool> UpdateNurseProfileAsync(string nurseId, NurseProfileUpdateDto update);
    }

    public class NurseProfileDto
    {
        public string NurseId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string WorkingHoursText { get; set; } = "9:00 AM - 9:00 PM";
        public string Department { get; set; } = "General Nursing";
        public string ProfileImageUri { get; set; } = string.Empty;
    }

    public class NurseProfileUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string WorkingHoursText { get; set; } = "9:00 AM - 9:00 PM";
        public string Department { get; set; } = "General Nursing";
        public string ProfileImageUri { get; set; } = string.Empty;
    }
}
