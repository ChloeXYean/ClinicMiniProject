using System;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IReportingService
    {
        Task<ReportingSummaryDto> GetDoctorReportingAsync(string? doctorId, DateTime start, DateTime end);
    }

    public sealed class ReportingSummaryDto
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public int TotalConsulted { get; init; }
        public int WalkInConsultation { get; init; }
        public int OnlineConsultation { get; init; }

        public ServiceTypeBreakdownDto ServiceType { get; init; } = new();

        public OnlineInquiryBreakdownDto OnlineInquiry { get; init; } = new();
    }

    public sealed class ServiceTypeBreakdownDto
    {
        public int GeneralConsultation { get; init; }
        public int FollowUpTreatment { get; init; }
        public int TestResultDiscussion { get; init; }
        public int VaccinationOrInjection { get; init; }

        public MedicalScreeningDto TotalMedicalScreening { get; init; } = new();
    }

    public sealed class MedicalScreeningDto
    {
        public int BloodTest { get; init; }
        public int BloodPressure { get; init; }
        public int SugarTest { get; init; }
    }

    public sealed class OnlineInquiryBreakdownDto
    {
        public int TotalInquiry { get; init; }
        public int Replied { get; init; }
    }
}
