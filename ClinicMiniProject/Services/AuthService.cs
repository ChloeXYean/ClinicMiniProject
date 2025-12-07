using ClinicMiniProject.Models;
using System.Collections.Generic;
using System.Linq;

namespace ClinicMiniProject.Services
{
    public class AuthService
    {
        private static List<Patient> _patients = new List<Patient>();
        private static List<Staff> _staffs = new List<Staff>();

        public bool RegisterPatient(Patient patient, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(patient.patient_IC) ||
                string.IsNullOrWhiteSpace(patient.patient_name) ||
                string.IsNullOrWhiteSpace(patient.password))
            {
                message = "Fields cannot be empty.";
                return false;
            }

            if (_patients.Any(p => p.patient_IC == patient.patient_IC))
            {
                message = "IC already registered.";
                return false;
            }

            _patients.Add(patient);
            return true;
        }

        public object Login(string id, string password, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
            {
                message = "Fields cannot be empty.";
                return null;
            }

            var patient = _patients.FirstOrDefault(p => p.patient_IC == id && p.password == password);
            if (patient != null) return patient;

            var staff = _staffs.FirstOrDefault(s => s.staff_ID == id && s.staff_password == password);
            if (staff != null) return staff;

            message = "Invalid credentials.";
            return null;
        }

        // testing only
        public void SeedStaff()
        {
            if (_staffs.Count == 0)
            {
                _staffs.Add(new Staff
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
