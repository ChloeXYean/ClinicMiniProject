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
            // Pull appointments within [start, end)
            var appts = (await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync(doctorId, start, end))
                ?.ToList() ?? new();

            // Define "consulted" as Completed or Consulted
            var consulted = appts
                .Where(a => a.status == "Completed" || a.status == "Consulted")
                .ToList();

            var onlineConsulted = consulted.Count(a => _appointmentService.IsAppUser(a.patient_IC));
            var walkInConsulted = consulted.Count - onlineConsulted;

            // TODO: link with database
            // Service type counters require:
            // - A service type field on Appointment OR
            // - A join table (Appointment -> Service)
            // Right now Appointment model doesn't contain service type, so all service type counters are 0.

            // TODO: link with database
            // Online inquiry + replied counters require an Inquiry/Chat/Message model/table.

            return new ReportingSummaryDto
            {
                Start = start,
                End = end,

                TotalConsulted = consulted.Count,
                WalkInConsultation = walkInConsulted,
                OnlineConsultation = onlineConsulted,

                ServiceType = new ServiceTypeBreakdownDto
                {
                    GeneralConsultation = 0,
                    FollowUpTreatment = 0,
                    TestResultDiscussion = 0,
                    VaccinationOrInjection = 0,
                    TotalMedicalScreening = new MedicalScreeningDto
                    {
                        BloodTest = 0,
                        BloodPressure = 0,
                        SugarTest = 0
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
