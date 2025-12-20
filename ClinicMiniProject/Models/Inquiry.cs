using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicMiniProject.Models
{
    [Table("inquiry")]
    public class Inquiry
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("inquiry_ID")]
        [StringLength(7)]
        public string InquiryId { get; set; } = string.Empty;

        [Required]
        [Column("patient_IC")]
        [StringLength(20)]
        public string PatientIc { get; set; } = string.Empty;

        [Required]
        [Column("doctor_ID")]
        [StringLength(4)]
        public string DoctorId { get; set; } = string.Empty;

        [Column("ask_datetime")]
        public DateTime AskDatetime { get; set; } = DateTime.Now;

        [Required]
        [Column("symptom_description")]
        public string SymptomDescription { get; set; } = string.Empty;

        [Column("inquiry_status")]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Replied', 'Closed'

        [Column("doc_reply")]
        public string? DoctorReply { get; set; }

        [Column("reply_datetime")]
        public DateTime? ReplyDatetime { get; set; }

        // --- Navigation Properties ---
        [ForeignKey(nameof(PatientIc))]
        public virtual Patient? Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual Staff? Doctor { get; set; }
    }
}