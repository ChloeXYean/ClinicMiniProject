using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IInquiryService
    {
        Task<IReadOnlyList<InquiryDto>> GetInquiriesAsync(string? query);
        Task<InquiryDto?> GetInquiryByIdAsync(string inquiryId);
        Task SendResponseAsync(string inquiryId, string doctorResponse);
        Task<bool> CreateInquiryAsync(InquiryDto inquiry);
    }

    public sealed class InquiryDto
    {
        public string InquiryId { get; init; } = string.Empty;
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public int PatientAge { get; init; }
        public string PatientGender { get; init; } = string.Empty;

        public string FullSymptomDescription { get; init; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public string? Image1 { get; init; }
        public string? Image2 { get; init; }
        public string? Image3 { get; init; }

        public string? DoctorResponse { get; set; }
    }
}
