using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ClinicMiniProject.Services
{
    public sealed class InquiryService : IInquiryService
    {
        private readonly AppDbContext _context;

        public InquiryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InquiryDto>> GetInquiriesByDoctorAsync(string doctorId, string? query = null)
        {
            System.Diagnostics.Debug.WriteLine($"=== GetInquiriesByDoctorAsync called for doctor: {doctorId} ===");
            
            var dbQuery = _context.Inquiries
                .Include(i => i.Patient) // Join with Patient table
                .Where(i => i.DoctorId == doctorId) // Filter by specific doctor
                .AsQueryable();

            // Debug: Check total inquiries before filtering
            var allInquiries = await _context.Inquiries.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total inquiries in database: {allInquiries.Count}");
            
            // Debug: Check inquiries for this doctor
            var doctorInquiries = allInquiries.Where(i => i.DoctorId == doctorId).ToList();
            System.Diagnostics.Debug.WriteLine($"Inquiries for doctor {doctorId}: {doctorInquiries.Count}");
            
            foreach (var inquiry in doctorInquiries)
            {
                System.Diagnostics.Debug.WriteLine($"  - Inquiry {inquiry.InquiryId} for patient {inquiry.PatientIc}, Doctor: {inquiry.DoctorId}");
            }

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
            System.Diagnostics.Debug.WriteLine($"Final filtered list count: {list.Count}");

            // Convert DB Model to DTO
            return list.Select(i => new InquiryDto
            {
                InquiryId = i.InquiryId,
                PatientIc = i.PatientIc,
                PatientName = i.Patient?.patient_name ?? "Unknown",
                PatientAge = 0, // Age not in DB, set 0 or calculate from IC if possible
                PatientGender = "N/A", // Gender not in DB
                FullSymptomDescription = i.SymptomDescription,
                Status = i.Status,
                DoctorResponse = i.DoctorReply ?? string.Empty,
                Image1 = null,
                Image2 = null,
                Image3 = null // Images not in SQL schema
            }).ToList();
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
                .Include(i => i.Patient) // Join with Patient table
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

            // Convert DB Model to DTO
            return list.Select(i => new InquiryDto
            {
                InquiryId = i.InquiryId,
                PatientIc = i.PatientIc,
                PatientName = i.Patient?.patient_name ?? "Unknown",
                PatientAge = 0, // Age not in DB, set 0 or calculate from IC if possible
                PatientGender = "N/A", // Gender not in DB
                FullSymptomDescription = i.SymptomDescription,
                Status = i.Status,
                DoctorResponse = i.DoctorReply ?? string.Empty,
                Image1 = null,
                Image2 = null,
                Image3 = null // Images not in SQL schema
            }).ToList();
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
            };
        }

        public async Task<bool> CreateInquiryAsync(InquiryDto dto)
        {
            if (dto == null) return false;

            // Generate ID if missing (Simple random logic)
            string newId = dto.InquiryId;
            if (string.IsNullOrEmpty(newId))
                newId = "I" + new Random().Next(100000, 999999).ToString();

            // Use the DoctorId from DTO if provided, otherwise use fallback
            string assignedDocId = dto.DoctorId;
            
            // Only use default doctor if no doctor was specified
            if (string.IsNullOrEmpty(assignedDocId))
            {
                var defaultDoc = await _context.Staffs.FirstOrDefaultAsync(s => s.isDoctor);
                assignedDocId = defaultDoc?.staff_ID ?? "S001";
            }

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
                .Include(i => i.Doctor) // Include doctor to get doctor name
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
                DoctorName = i.Doctor?.staff_name ?? "Unknown", // Add doctor name
                Image1 = null,
                Image2 = null,
                Image3 = null
            }).ToList();
        }
    }
}
