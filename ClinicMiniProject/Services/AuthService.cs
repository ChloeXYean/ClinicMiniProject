using ClinicMiniProject.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;


namespace ClinicMiniProject.Services
{
    public class AuthService : IAuthService
    {
        private static List<ClinicMiniProject.Models.Patient> _patients = new List<ClinicMiniProject.Models.Patient>();
        private static List<ClinicMiniProject.Models.Staff> _staffs = new List<ClinicMiniProject.Models.Staff>();

        private static ClinicMiniProject.Models.Staff? _currentStaff;

        public bool RegisterPatient(ClinicMiniProject.Models.Patient patient, out string message)
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

            if (_patients.Any(p => p.patient_IC == patient.patient_IC))
            {
                message = "IC already registered. Please login your account.";
                return false;
            }

            _patients.Add(patient);
            return true;
        }

        public object Login(string patient_IC, string password, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(patient_IC))
            {
                message = "Ic number cannot be empty.";
                return null;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Password cannot be empty.";
                return null;
            }

            var patient = _patients.FirstOrDefault(p => p.patient_IC == patient_IC && p.password == password);
            if (patient != null) return patient;

            var staff = _staffs.FirstOrDefault(s => s.staff_ID == patient_IC && s.staff_password == password);
            if (staff != null)
            {
                _currentStaff = staff;
                return staff;
            }

            message = "Account not found. Please try again or create a new account.";
            return null;
        }

        public ClinicMiniProject.Models.Staff GetCurrentUser()
        {
            return _currentStaff;
        }

        public string GetDoctorName(string doctorId)
        {
            var staff = _staffs.FirstOrDefault(s => s.staff_ID == doctorId);
            return staff?.staff_name ?? string.Empty;
        }

        public void Logout()
        {
            _currentStaff = null;
        }

        // TODO: testing only later need to delete
        public void SeedStaff()
        {
            if (_staffs.Count == 0)
            {
                _staffs.Add(new ClinicMiniProject.Models.Staff
                {
                    staff_ID = "S001",
                    staff_name = "Dr Ali",
                    staff_password = "1234",
                    isDoctor = true,
                    specialities = "General"
                });
            }
        }
    }
}
