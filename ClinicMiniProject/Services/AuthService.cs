using ClinicMiniProject.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;


namespace ClinicMiniProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private static Staff? _currentStaff;

        public AuthService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public bool RegisterPatient(Patient patient, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(patient.patient_IC))
            {
                message = "Ic number cannot be empty.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(patient.patient_name))
            {
                message = "Full name cannot be empty.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(patient.password))
            {
                message = "Password cannot be empty.";
                return false;
            }

            bool exists = _context.Patients.Any(p => p.patient_IC == patient.patient_IC);
            if (exists)
            {
                message = "IC already registered. Please login to your account.";
                return false;
            }

            _context.Patients.Add(patient);
            _context.SaveChanges();
            return true;
        }

        public object Login(string patient_IC, string password, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(patient_IC) || string.IsNullOrWhiteSpace(password))
            {
                message = "Ic number and password cannot be empty.";
                return null;
            }

            var patient = _context.Patients.FirstOrDefault(p => p.patient_IC == patient_IC && p.password == password);
            if (patient != null) return patient;

            //var staff = _context.Staffs.FirstOrDefault(s => s.staff_ID == patient_IC && s.staff_password == password);
            var staff = _context.Staffs.FirstOrDefault(s => s.staff_ID == patient_IC);
            if (staff != null)
            {
                _currentStaff = staff;
                return staff;
            } 

            message = "Account not found. Please try again or create a new account.";
            return null;
        }

        public Staff GetCurrentUser()
        {
            return _currentStaff;
        }

        public string GetDoctorName(string doctorId)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.staff_ID == doctorId);
            return staff?.staff_name ?? string.Empty;
        }

        public void Logout()
        {
            _currentStaff = null;
        }

        // TODO: testing only later need to delete
        public void SeedStaff()
        {
            if (!_context.Staffs.Any())
            {
                // Add Doctor
                _context.Staffs.Add(new Staff
                {
                    staff_ID = "D001",
                    staff_name = "Dr. Ali",
                    //password = "password123", // Make sure to set this!
                    isDoctor = true,
                    specialities = "General"
                });

                // Add Nurse
                _context.Staffs.Add(new Staff
                {
                    staff_ID = "N001",
                    staff_name = "Nurse Sarah",
                    //password = "password123",
                    isDoctor = false, // This will route to NurseHomePage
                    specialities = "Nursing"
                });

                _context.SaveChanges();
            }
        }
    }
}
