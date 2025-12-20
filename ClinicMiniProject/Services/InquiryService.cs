using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Dtos; // Ensure you have this namespace
using ClinicMiniProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicMiniProject.Services
{
    public class InquiryService : IInquiryService
    {
        private readonly AppDbContext _context;

        public InquiryService(AppDbContext context)
        {
            _context = context;
        }

        // --- 1. DOCTOR SIDE METHODS ---

        public async Task<IReadOnlyList<InquiryDto>> GetInquiriesByDoctorAsync(string doctorId, string? query = null)
        {
            try
            {
                var dbQuery = _context.Inquiries
                    .Include(i => i.Patient)
                    .Where(i => i.DoctorId == doctorId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    string q = query.Trim();
                    dbQuery = dbQuery.Where(i =>
                        i.PatientIc.Contains(q) ||
                        (i.Patient != null && i.Patient.patient_name.Contains(q)) ||
                        i.SymptomDescription.Contains(q) ||
                        i.Status.Contains(q));
                }

                var list = await dbQuery.OrderByDescending(i => i.AskDatetime).ToListAsync();

                return MapToDtoList(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetInquiriesByDoctorAsync: {ex.Message}");
                return new List<InquiryDto>();
            }
        }

        public async Task SendResponseAsync(string inquiryId, string doctorResponse)
        {
            var inquiry = await _context.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == inquiryId);
            if (inquiry == null) return;

            inquiry.DoctorReply = doctorResponse;
            inquiry.Status = "Replied";
            inquiry.ReplyDatetime = DateTime.Now;

            _context.Inquiries.Update(inquiry);
            await _context.SaveChangesAsync();
        }

        // --- 2. GENERAL / ADMIN METHODS ---

        public async Task<IReadOnlyList<InquiryDto>> GetInquiriesAsync(string? query)
        {
            var dbQuery = _context.Inquiries
                .Include(i => i.Patient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                string q = query.Trim();
                dbQuery = dbQuery.Where(i =>
                    i.PatientIc.Contains(q) ||
                    (i.Patient != null && i.Patient.patient_name.Contains(q)) ||
                    i.SymptomDescription.Contains(q) ||
                    i.Status.Contains(q));
            }

            var list = await dbQuery.OrderByDescending(i => i.AskDatetime).ToListAsync();
            return MapToDtoList(list);
        }

        public async Task<InquiryDto?> GetInquiryByIdAsync(string inquiryId)
        {
            var i = await _context.Inquiries
                .Include(x => x.Patient)
                .FirstOrDefaultAsync(x => x.InquiryId == inquiryId);

            if (i == null) return null;

            return new InquiryDto
            {
                InquiryId = i.InquiryId,
                PatientIc = i.PatientIc,
                PatientName = i.Patient?.patient_name ?? "Unknown",
                FullSymptomDescription = i.SymptomDescription,
                Status = i.Status,
                DoctorResponse = i.DoctorReply ?? string.Empty
                // Add other properties if needed
            };
        }

        // --- 3. PATIENT SIDE METHODS ---

        public async Task<bool> CreateInquiryAsync(InquiryDto dto)
        {
            if (dto == null) return false;

            // Generate ID if missing
            string newId = dto.InquiryId;
            if (string.IsNullOrEmpty(newId))
                newId = "I" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            // Assign a default doctor if your DTO doesn't select one
            // (Since DoctorId is likely a Foreign Key, we need a valid one)
            string assignedDocId = "S001"; // Default fallback
            var defaultDoc = await _context.Staffs.FirstOrDefaultAsync(s => s.isDoctor);
            if (defaultDoc != null)
                assignedDocId = defaultDoc.staff_ID;

            var newInquiry = new Inquiry
            {
                InquiryId = newId,
                PatientIc = dto.PatientIc,
                DoctorId = assignedDocId,
                SymptomDescription = dto.FullSymptomDescription,
                AskDatetime = DateTime.Now,
                Status = "Pending"
            };

            _context.Inquiries.Add(newInquiry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<InquiryDto>> GetInquiriesByPatientIcAsync(string patientIc)
        {
            // FIX: Now queries the Database instead of the static list
            var list = await _context.Inquiries
                .Include(i => i.Patient)
                .Where(i => i.PatientIc == patientIc)
                .OrderByDescending(i => i.AskDatetime)
                .ToListAsync();

            return MapToDtoList(list);
        }

        // --- HELPER MAPPING METHOD ---

        private List<InquiryDto> MapToDtoList(List<Inquiry> inquiries)
        {
            return inquiries.Select(i => new InquiryDto
            {
                InquiryId = i.InquiryId,
                PatientIc = i.PatientIc,
                PatientName = i.Patient?.patient_name ?? "Unknown",
                PatientAge = 0, // Calculate from IC if needed
                PatientGender = "N/A", // Fetch from Patient table if needed
                FullSymptomDescription = i.SymptomDescription,
                Status = i.Status,
                DoctorResponse = i.DoctorReply ?? string.Empty,
                Image1 = null,
                Image2 = null,
                Image3 = null
            }).ToList();
        }
    }
}