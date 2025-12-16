using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ClinicMiniProject
{
    public class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<DocAvailable> DocAvailables { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        //string server = Environment.GetEnvironmentVariable("DB_SERVER_IP") ?? "172.16.59.30";
        //        //string server = Environment.GetEnvironmentVariable("DB_SERVER_IP") ?? "localhost";
        //        //string connStr = $"Server={server};Port=3306;Database=testdb;Uid=root;Pwd=123456;Charset=utf8mb4;";
        //        string connStr = $"Server=localhost;Port=3306;Database=testdb;Uid=root;Pwd=123456;Charset=utf8mb4;";

        //        optionsBuilder.UseMySql(
        //            connStr,
        //            ServerVersion.AutoDetect(connStr),
        //            options => options.EnableRetryOnFailure()
        //        );
        //    }
        //}
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        var connStr =
        //            "Server=127.0.0.1;" +
        //            "Port=3306;" +
        //            "Database=testdb;" +
        //            "Uid=root;" +
        //            "Pwd=123456;" +
        //            "SslMode=None;" +
        //            "Charset=utf8mb4;";

        //        optionsBuilder.UseMySql(
        //            connStr,
        //            new MySqlServerVersion(new Version(9, 0, 0)),
        //            options => options.EnableRetryOnFailure()
        //        );
        //    }
        //}

        // ClinicMiniProject/AppDbContext.cs

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default to localhost for Windows
                string serverIp = "127.0.0.1";

                // If running on Android, change IP to 10.0.2.2 (which points to your PC)
#if ANDROID
                serverIp = "10.0.2.2";
#endif

                var connStr =
                    $"Server={serverIp};" + // Use the variable here
                    "Port=3306;" +
                    "Database=testdb;" +
                    "Uid=root;" +
                    "Pwd=123456;" +
                    "SslMode=None;" +
                    "Charset=utf8mb4;";

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
                //.WithOne(d => d.Staff)
                .WithOne()
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

            // Ensure all column names match exactly with the database schema
            //foreach (var entity in modelBuilder.Model.GetEntityTypes())
            //{
            //    foreach (var property in entity.GetProperties())
            //    {
            //        property.SetColumnName(property.GetColumnName());
            //    }
            //}
        }
    }
}
