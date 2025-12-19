using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IDoctorProfileService
    {
        Task<DoctorProfileDto?> GetDoctorProfileAsync(string doctorId);

        Task UpdateDoctorProfileAsync(string doctorId, DoctorProfileUpdateDto update);
    }

    public sealed class DoctorProfileDto
    {
        public string DoctorId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string PhoneNo { get; init; } = string.Empty;

        public string WorkingHoursText { get; init; } = string.Empty;

        public IReadOnlyList<string> ServicesProvided { get; init; } = new List<string>();

        public string ProfileImageUri { get; init; } = string.Empty;
    }

    public sealed class DoctorProfileUpdateDto
    {
        public string? Name { get; init; }
        public string? PhoneNo { get; init; }
        public string? WorkingHoursText { get; init; }
        public IReadOnlyList<string>? ServicesProvided { get; init; }
        public string? ProfileImageUri { get; init; }
    }
}
