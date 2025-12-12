using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ClinicMiniProject
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<DocAvailable> DocAvailables { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string server = Environment.GetEnvironmentVariable("DB_SERVER_IP") ?? "127.0.0.1";
                string connStr = $"Server={server};Port=3306;Database=testdb;Uid=root;Pwd=123456;Charset=utf8mb4;";

                optionsBuilder.UseMySql(
                    connStr,
                    ServerVersion.AutoDetect(connStr),
                    options => options.EnableRetryOnFailure()
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Availability)
                .WithOne(d => d.Staff)
                .HasForeignKey<DocAvailable>(d => d.doctorID);

            // Ensure one-to-one relationship between Staff and DocAvailable
            modelBuilder.Entity<DocAvailable>()
                .HasIndex(d => d.doctorID)
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.patientIC)
                .HasPrincipalKey(p => p.patientIC);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.staffID);

            // Ensure all column names match exactly with the database schema
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName());
                }
            }
        }
    }
}
