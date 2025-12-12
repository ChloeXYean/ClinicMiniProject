using ClinicMiniProject.Models;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IAuthService
    {
        object Login(string patient_IC, string password, out string message);
        bool RegisterPatient(Patient patient, out string message);

        Staff GetCurrentUser();
        string GetDoctorName(string doctorId);
        void Logout();

        void SeedStaff();
        // ... other existing methods
    }
}