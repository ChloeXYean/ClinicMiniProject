using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ClinicMiniProject
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<DocAvailable> DocAvailables { get; set; } = null!;
        public DbSet<Inquiry> Inquiries { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // local database
                /*string serverIp = "localhost";
                    var connStr =
                    $"Server={serverIp};" + // Use the variable here
                    "Port=3306;" +
                    "Database=testdb;" +
                    "Uid=root;" +
                    "Pwd=123456;" +
                    "SslMode=None;" +
                    "Charset=utf8mb4;"; */

                //Online db
                string serverip = "ballast.proxy.rlwy.net";
                var connStr =
                    $"server={serverip};" + // use the variable here
                    "port=19463;" +
                    "database=railway;" +
                    "uid=root;" +
                    "pwd=NrIvCewJcTGAPqmOXyoziksWgwoQmaQd;" +
                    "sslmode=Required;" +
                    "charset=utf8mb4;";


                optionsBuilder.UseMySql(
                    connStr,
                    new MySqlServerVersion(new Version(9, 0, 0)),
                    options => options.EnableRetryOnFailure()
                );
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Availability)
                .WithOne(d => d.Staff)
                .HasForeignKey<DocAvailable>(d => d.staff_ID);

            // Ensure one-to-one relationship between Staff and DocAvailable
            modelBuilder.Entity<DocAvailable>()
                .HasIndex(d => d.staff_ID)
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.patient_IC)
                .HasPrincipalKey(p => p.patient_IC);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.staff_ID);

            modelBuilder.Entity<Inquiry>(entity =>
            {
                entity.HasIndex(e => e.InquiryId).IsUnique(); // inquiry_ID unique

                entity.HasOne(d => d.Patient)
                      .WithMany()
                      .HasForeignKey(d => d.PatientIc)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Doctor)
                      .WithMany()
                      .HasForeignKey(d => d.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

            
            });
        }
    }
}
