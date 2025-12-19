//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ClinicMiniProject
//{
//    [Table("patient")]
//    public class Patient
//    {
//        // Primary Key
//        [Key]
//        [Column("patient_IC")]
//        public string patient_IC { get; set; }

//        [Column("patient_name")]
//        [Required]
//        [StringLength(100)]
//        public string patient_name { get; set; } = string.Empty;

//        [Column("patient_contact")]
//        [Required]
//        [StringLength(15)]
//        public string patient_contact { get; set; } = string.Empty;

//        [Column("patient_email")]
//        [StringLength(100)]
//        public string? patient_email { get; set; } = null;

//        [Column("password")]
//        public string? password { get; set; } = null;

//        [Column("isAppUser")]
//        [Required]
//        public bool isAppUser { get; set; } = false;

//        public List<Appointment> Appointments { get; set; } = new();
//    }

//    [Table("staff")]
//    public class Staff
//    {
//        [Key]
//        [Column("staff_ID")]
//        [StringLength(4)]
//        public string staff_ID { get; set; } = string.Empty;

//        [Column("staff_name")]
//        [Required, StringLength(100)]
//        public string staff_name { get; set; } = string.Empty;

//        [Column("staff_contact")]
//        [Required, StringLength(15)]
//        public string staff_contact { get; set; } = string.Empty;

//        [Column("password")]
//        [StringLength(15)]
//        public string? password { get; set; }

//        [Column("specialities")]
//        [StringLength(100)]
//        public string? specialities { get; set; }

//        [Column("isDoctor")]
//        [Required]
//        public bool isDoctor { get; set; }
        
//        //Password

//        public List<Appointment> Appointments { get; set; } = new();
//        public DocAvailable? Availability { get; set; }
//    }

//    [Table("appointment")]
//    public class Appointment
//    {
//        [Key]
//        [Column("id")]
//        public int id { get; set; }

//        [Column("appointment_ID")]
//        [StringLength(7)]
//        public string? appointment_ID { get; set; } = null;

//        [Column("bookedAt")]
//        public DateTime bookedAt { get; set; } = DateTime.Now;

//        [Column("appointmentAt")]
//        [Required]
//        public DateTime? appointedAt { get; set; }

//        [Column("staff_ID")]
//        public string staff_ID { get; set; } = string.Empty;

//        [Column("patient_IC")]
//        public string patient_IC { get; set; }

//        [Column("status")]
//        public string status { get; set; } = "Pending";

//        public Staff Staff { get; set; } = null!;
//        public Patient Patient { get; set; } = null!;
//    }

//    [Table("docAvailable")]
//    public class DocAvailable
//    {
//        [Key]
//        [Column("doctor_ID")]
//        public string staff_ID { get; set; } = string.Empty;
//        public bool Monday { get; set; } = false;
//        public bool Tuesday { get; set; } = false;
//        public bool Wednesday { get; set; } = false;
//        public bool Thursday { get; set; } = false;
//        public bool Friday { get; set; } = false;
//        public bool Saturday { get; set; } = false;
//        public bool Sunday { get; set; } = false;


//        public Staff Staff { get; set; } = null!;

//        public bool IsAvailable(DayOfWeek day) =>
//         day switch
//         {
//             DayOfWeek.Monday => Monday,
//             DayOfWeek.Tuesday => Tuesday,
//             DayOfWeek.Wednesday => Wednesday,
//             DayOfWeek.Thursday => Thursday,
//             DayOfWeek.Friday => Friday,
//             DayOfWeek.Saturday => Saturday,
//             DayOfWeek.Sunday => Sunday,
//             _ => false,
//         };
//    }
//}
