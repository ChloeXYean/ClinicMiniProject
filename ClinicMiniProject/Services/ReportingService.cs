using System;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IAppointmentService _appointmentService;

        public ReportingService(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<ReportingSummaryDto> GetDoctorReportingAsync(string? doctorId, DateTime start, DateTime end)
        {
            // 1. Fetch Data
            // If doctorId is null/empty, this service method should return ALL appointments (check your AppointmentService implementation)
            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, start, end))
                ?.ToList() ?? new();

            // 2. Filter: Only count "Completed" or "Consulted" appointments
            var consulted = appts
                .Where(a => a.status == "Completed" || a.status == "Consulted")
                .ToList();

            // 3. Calculate Online vs Walk-in
            // Assuming you have a way to check if it was an App User (Online)
            // If IsAppUser is not available, you might check if 'patient_IC' exists in Patient table
            var onlineConsulted = consulted.Count(a => _appointmentService.IsAppUser(a.patient_IC));
            var walkInConsulted = consulted.Count - onlineConsulted;

            // 4. Calculate Service Types (Logic Added!)
            // We use case-insensitive matching to be safe
            int CountService(string type) => consulted.Count(a => string.Equals(a.service_type, type, StringComparison.OrdinalIgnoreCase));

            return new ReportingSummaryDto
            {
                Start = start,
                End = end,

                TotalConsulted = consulted.Count,
                WalkInConsultation = walkInConsulted,
                OnlineConsultation = onlineConsulted,

                ServiceType = new ServiceTypeBreakdownDto
                {
                    GeneralConsultation = CountService("General Consultation"),
                    FollowUpTreatment = CountService("Follow Up Treatment"),
                    TestResultDiscussion = CountService("Test Result Discussion"),
                    VaccinationOrInjection = CountService("Vaccination/Injection"), // Check your DB string matches this

                    // For Medical Screening, we sum up the subtypes
                    TotalMedicalScreening = new MedicalScreeningDto
                    {
                        BloodTest = CountService("Blood Test"),
                        BloodPressure = CountService("Blood Pressure"),
                        SugarTest = CountService("Sugar Test")
                    }
                },

                // Online Inquiries are usually handled by InquiryService, not here, 
                // so we leave this as 0 or handled by the ViewModel separately.
                OnlineInquiry = new OnlineInquiryBreakdownDto
                {
                    TotalInquiry = 0,
                    Replied = 0
                }
            };
        }
    }
}