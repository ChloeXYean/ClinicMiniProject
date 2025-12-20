using Org.BouncyCastle.Bcpg;

namespace ClinicMiniProject.Dtos
{
    public class InquiryDto
    {
        public string InquiryId { get; init; } = string.Empty;
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public int PatientAge { get; init; }
        public string PatientGender { get; init; } = string.Empty;

        public string DoctorId {  get; init; } 

        public string FullSymptomDescription { get; init; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public string? Image1 { get; init; }
        public string? Image2 { get; init; }
        public string? Image3 { get; init; }

        public string? DoctorResponse { get; set; }
    }
}