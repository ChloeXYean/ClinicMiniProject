using ClinicMiniProject.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClinicMiniProject.Models;


namespace ClinicMiniProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private static Staff? _currentStaff;
        private static Patient? _currentPatient;

        public AuthService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        #region Validation Methods

        public bool ValidateEmail(string email, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(email))
            {
                message = "Email cannot be empty.";
                return false;
            }

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                message = "Invalid email format. Please follow format: example@domain.com";
                return false;
            }

            return true;
        }

        public bool ValidatePhoneNumber(string phone, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(phone))
            {
                message = "Phone number cannot be empty.";
                return false;
            }

            // Remove dashes and spaces for validation
            var cleanPhone = phone.Replace("-", "").Replace(" ", "");

            if (cleanPhone.Length != 10 || !Regex.IsMatch(cleanPhone, @"^\d{10}$"))
            {
                message = "Invalid phone number format. Please follow format: 0123456789 or 012-3456789";
                return false;
            }

            return true;
        }

        public string FormatPhoneNumber(string phone)
        {
            // Remove dashes and spaces, return clean 10-digit format
            return phone.Replace("-", "").Replace(" ", "");
        }

        public bool ValidateICNumber(string ic, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(ic))
            {
                message = "IC number cannot be empty.";
                return false;
            }

            // Remove dashes for validation
            var cleanIc = ic.Replace("-", "");

            if (cleanIc.Length != 12 || !Regex.IsMatch(cleanIc, @"^\d{12}$"))
            {
                message = "Invalid IC number format. Please follow format: 123456121234 or 123446-12-1234";
                return false;
            }

            return true;
        }

        public string FormatICNumber(string ic)
        {
            // Remove dashes, return clean 12-digit format
            return ic.Replace("-", "");
        }

        public bool ValidatePassword(string password, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Password cannot be empty.";
                return false;
            }

            if (password.Length < 6)
            {
                message = "Password must be at least 6 characters long.";
                return false;
            }

            return true;
        }

        #endregion

        // ... RegisterPatient omitted ...

        public bool RegisterPatient(Patient patient, out string message)
        {
            message = "";

            // Validate full name
            if (string.IsNullOrWhiteSpace(patient.patient_name))
            {
                message = "Full name cannot be empty.";
                return false;
            }

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(patient.patient_email))
            {
                if (!ValidateEmail(patient.patient_email, out message))
                {
                    return false;
                }
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(patient.patient_contact))
            {
                if (!ValidatePhoneNumber(patient.patient_contact, out message))
                {
                    return false;
                }
                // Format phone number for database storage
                patient.patient_contact = FormatPhoneNumber(patient.patient_contact);
            }

            // Validate IC number format
            if (!ValidateICNumber(patient.patient_IC, out message))
            {
                return false;
            }

            // Validate password
            if (!ValidatePassword(patient.password, out message))
            {
                return false;
            }

            try
            {
                // Format IC number for database storage and duplicate check
                string formattedIc = FormatICNumber(patient.patient_IC);

                // Check for existing IC using formatted version
                bool exists = _context.Patients.Any(p => p.patient_IC == formattedIc);
                if (exists)
                {
                    message = "IC already registered. Please login to your account.";
                    return false;
                }

                // Store the formatted version
                patient.patient_IC = formattedIc;

                _context.Patients.Add(patient);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Handle different types of exceptions more specifically
                if (ex is InvalidOperationException || ex.InnerException is InvalidOperationException)
                {
                    message = "Database connection error. Please check your internet connection and try again.";
                }
                else if (ex is InvalidCastException)
                {
                    message = $"Database type mismatch during registration: {ex.Message}. The database may have patient_IC as a numeric type.";
                }
                else
                {
                    message = $"Registration failed: {ex.Message}";
                }
                return false;
            }
        }

        public object Login(string patient_IC, string password, out string message)
        {
            message = "";

            // Validate IC number/ID first
            if (string.IsNullOrWhiteSpace(patient_IC))
            {
                message = "IC number/ID cannot be empty.";
                return null;
            }

            // Validate IC number format for patients
            if (!ValidateICNumber(patient_IC, out message))
            {
                // If IC format is invalid, check if it's a staff ID (could be different format)
                // Staff IDs might have different formats, so we'll proceed with staff check
                message = ""; // Clear message for staff check
            }
            else
            {
                // Format IC number for database lookup
                patient_IC = FormatICNumber(patient_IC);
            }

            // Validate password second
            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Password cannot be empty.";
                return null;
            }

            if (!ValidatePassword(password, out message))
            {
                return null;
            }

            var patient = _context.Patients.FirstOrDefault(p => p.patient_IC == patient_IC && p.password == password);
            if (patient != null)
            {
                _currentPatient = patient;
                return patient;
            }

            var staff = _context.Staffs.FirstOrDefault(s => s.staff_ID == patient_IC && s.password == password);
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

        public Patient GetCurrentPatient()
        {
            return _currentPatient;
        }

        public string GetDoctorName(string doctorId)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.staff_ID == doctorId);
            return staff?.staff_name ?? string.Empty;
        }

        public void Logout()
        {
            _currentStaff = null;
            _currentPatient = null;
        }

    }
}
