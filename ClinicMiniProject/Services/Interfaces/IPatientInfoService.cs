using System.Threading.Tasks;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IPatientInfoService
    {
        Task<PatientInfoDto?> GetPatientInfoAsync(string patientIc);
    }

    public sealed class PatientInfoDto
    {
        public string PatientIc { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string PhoneNo { get; init; } = string.Empty;
        public string ServiceType { get; init; } = string.Empty;
        public string PatientType { get; init; } = string.Empty;
        public string RegisteredTime { get; init; } = string.Empty;
    }
}
