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

        [Column("password")]
        public string? password { get; set; }

        [Column("staff_contact")]
        [Required, StringLength(15)]
        public string staff_contact { get; set; } = string.Empty;

        [Column("isDoctor")]
        [Required]
        public bool isDoctor { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
        public DocAvailable? Availability { get; set; }
    }
}

