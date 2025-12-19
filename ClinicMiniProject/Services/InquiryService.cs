using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public sealed class InquiryService : IInquiryService
    {
        private static readonly List<InquiryDto> _inquiries = new()
        {
            new InquiryDto
            {
                InquiryId = "INQ001",
                PatientIc = "P0001",
                PatientName = "Alex",
                PatientAge = 28,
                PatientGender = "Male",
                FullSymptomDescription = "Fever and sore throat for 2 days. Mild headache. No shortness of breath.",
                Status = "Pending",
                Image1 = null,
                Image2 = null,
                Image3 = null
            },
            new InquiryDto
            {
                InquiryId = "INQ002",
                PatientIc = "P0002",
                PatientName = "Ali",
                PatientAge = 34,
                PatientGender = "Male",
                FullSymptomDescription = "Skin rash on both arms after outdoor activity. Itchy, no pain.",
                Status = "Replied",
                DoctorResponse = "Please keep the area clean and avoid scratching. If it worsens, visit clinic.",
                Image1 = null,
                Image2 = null,
                Image3 = null
            },
            new InquiryDto
            {
                InquiryId = "INQ003",
                PatientIc = "P0003",
                PatientName = "Aina",
                PatientAge = 22,
                PatientGender = "Female",
                FullSymptomDescription = "Stomach discomfort after meals for 1 week. Occasional nausea.",
                Status = "Pending",
                Image1 = null,
                Image2 = null,
                Image3 = null
            }
        };

        public async Task<IReadOnlyList<InquiryDto>> GetInquiriesAsync(string? query)
        {
            await Task.Yield();

            if (string.IsNullOrWhiteSpace(query))
                return _inquiries.OrderByDescending(i => i.Status == "Pending").ToList();

            var q = query.Trim();
            return _inquiries
                .Where(i =>
                    (!string.IsNullOrWhiteSpace(i.PatientName) && i.PatientName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(i.FullSymptomDescription) && i.FullSymptomDescription.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(i.Status) && i.Status.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<InquiryDto?> GetInquiryByIdAsync(string inquiryId)
        {
            await Task.Yield();
            return _inquiries.FirstOrDefault(i => string.Equals(i.InquiryId, inquiryId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SendResponseAsync(string inquiryId, string doctorResponse)
        {
            await Task.Yield();

            var inquiry = _inquiries.FirstOrDefault(i => string.Equals(i.InquiryId, inquiryId, StringComparison.OrdinalIgnoreCase));
            if (inquiry == null)
                return;

            inquiry.DoctorResponse = doctorResponse;
            inquiry.Status = "Replied";
        }

        public async Task<bool> CreateInquiryAsync(InquiryDto inquiry)
        {
            await Task.Yield();
            if (inquiry == null) return false;

            _inquiries.Add(inquiry);
            return true;
        }
    }
}
