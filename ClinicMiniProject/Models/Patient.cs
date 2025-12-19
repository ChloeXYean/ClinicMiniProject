using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicMiniProject.Models
{
    [Table("patient")]
    public class Patient
    {
        [Key]
        [Column("patient_IC")]
        public string patient_IC { get; set; } // Changed to long to match your DbTables definition

        [Column("patient_name")]
        [Required]
        [StringLength(100)]
        public string patient_name { get; set; } = string.Empty;

        [Column("patient_contact")]
        [Required]
        [StringLength(15)]
        public string patient_contact { get; set; } = string.Empty;

        [Column("patient_email")]
        [StringLength(100)]
        public string? patient_email { get; set; } = null;

        [Column("password")]
        public string? password { get; set; } = null;

        [Column("isAppUser")]
        [Required]
        public bool isAppUser { get; set; } = false;

        public List<Appointment> Appointments { get; set; } = new();
    }
}