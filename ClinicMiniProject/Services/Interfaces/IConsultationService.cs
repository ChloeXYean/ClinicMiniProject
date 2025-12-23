using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicMiniProject.Dtos;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IConsultationService
    {
        Task<IEnumerable<ConsultationQueueDto>> GetConsultationQueueAsync(string doctorId, DateTime date);
        Task<ConsultationDetailsDto?> GetCurrentConsultationDetailsAsync(string doctorId, DateTime now);

        Task<IEnumerable<PatientLookupDto>> SearchPatientsByNameAsync(string nameQuery);

        Task<ConsultationDetailsDto?> GetConsultationDetailsByAppointmentIdAsync(string appointmentId);

        Task StartConsultationAsync(string appointmentId, string doctorId);

        Task EndConsultationAsync(string appointmentId, string doctorRemark, string? nurseRemark);

        Task UpdateRemarksAsync(string appointmentId, string docRemark, string nurseRemark);
        
        Task UpdateConsultationRemarksAsync(string appointmentId, string newRemarks);
    }

    public sealed class ConsultationDetailsDto
    {
        public string AppointmentId { get; init; } = string.Empty;
        public DateTime Date { get; init; }
        public DateTime AppointedTimeSlot { get; init; }
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public string PatientPhone { get; init; } = string.Empty;
        public string SelectedServiceType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string DoctorRemark { get; set; } = string.Empty;
        public string NurseRemark { get; set; } = string.Empty;
    }

    public sealed class PatientLookupDto
    {
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public string PatientPhone { get; init; } = string.Empty;
    }
}
