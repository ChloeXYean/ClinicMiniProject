using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicMiniProject.Models
{
    [Table("staff")]
    public class Staff
    {
        [Key]
        [Column("staff_ID")]
        [StringLength(4)]
        public string staff_ID { get; set; } = string.Empty;

        [Column("staff_name")]
        [Required, StringLength(100)]
        public string staff_name { get; set; } = string.Empty;

        // Added this field which was missing in DbTables but present in logic
        [Column("staff_password")]
        public string? staff_password { get; set; }

        [Column("staff_contact")]
        [Required, StringLength(15)]
        public string staff_contact { get; set; } = string.Empty;

        [Column("specialities")]
        [StringLength(100)]
        public string? specialities { get; set; }

        [Column("isDoctor")]
        [Required]
        public bool isDoctor { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
        public DoctorAvailability? Availability { get; set; }
    }
}

