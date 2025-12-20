using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicMiniProject.Dtos;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IInquiryService
    {
        Task<IReadOnlyList<InquiryDto>> GetInquiriesAsync(string? query);
        Task<IReadOnlyList<InquiryDto>> GetInquiriesByDoctorAsync(string doctorId, string? query = null);
        Task<InquiryDto?> GetInquiryByIdAsync(string inquiryId);
        Task SendResponseAsync(string inquiryId, string doctorResponse);
        Task<bool> CreateInquiryAsync(InquiryDto inquiry);
        Task<IReadOnlyList<InquiryDto>> GetInquiriesByPatientIcAsync(string patientIc);
    }
}
