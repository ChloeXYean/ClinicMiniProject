using System.Threading.Tasks;

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
        public string Department { get; set; } = "General Nursing";
        public string ProfileImageUri { get; set; } = string.Empty;
        public string ICNumber { get; set; } = string.Empty;
    }

    public class NurseProfileUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Department { get; set; } = "General Nursing";
        public string ProfileImageUri { get; set; } = string.Empty;
        public string ICNumber { get; set; } = string.Empty;
    }


}