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

            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, start, end))
                ?.ToList() ?? new();

            var consulted = appts
                .Where(a => a.status == "Completed" || a.status == "Consulted")
                .ToList();


            var onlineConsulted = consulted.Count(a => _appointmentService.IsAppUser(a.patient_IC));
            var walkInConsulted = consulted.Count - onlineConsulted;

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
                    VaccinationOrInjection = CountService("Vaccination/Injection"), 

                    // For Medical Screening, we sum up the subtypes
                    TotalMedicalScreening = new MedicalScreeningDto
                    {
                        BloodTest = CountService("Blood Test"),
                        BloodPressure = CountService("Blood Pressure"),
                        SugarTest = CountService("Sugar Test")
                    }
                },

                OnlineInquiry = new OnlineInquiryBreakdownDto
                {
                    TotalInquiry = 0,
                    Replied = 0
                }
            };
        }
    }
}