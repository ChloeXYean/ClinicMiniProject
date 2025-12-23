using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicMiniProject.Models
{
    [Table("appointment")]
    public class Appointment
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("appointment_ID")]
        [StringLength(7)]
        public string? appointment_ID { get; set; } = null;

        [Column("bookedAt")]
        public DateTime bookedAt { get; set; } = DateTime.Now;

        [Column("appointedAt")]
        [Required]
        public DateTime? appointedAt { get; set; }

        [Column("staff_ID")]
        public string staff_ID { get; set; } = string.Empty;

        [Column("patient_IC")]
        public string patient_IC { get; set; }

        [Column("service_type")]
        public string service_type { get; set; } = "General Consultation";

        [Column("status")]
        public string status { get; set; } = "Pending";

        [Column("doctor_remark")]
        public string? doc_remark { get; set; }

        [Column("nurse_remark")]
        public string? nurse_remark { get; set; }

        public Staff Staff { get; set; } = null!;
        public Patient Patient { get; set; } = null!;


    }
}