using ClinicMiniProject.Models;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IAuthService
    {
        Task<object> LoginAsync(string patient_IC, string password);
        object Login(string patient_IC, string password, out string message);
        bool RegisterPatient(Patient patient, out string message);

        Staff GetCurrentUser();
        Patient GetCurrentPatient();
        string GetDoctorName(string doctorId);
        void Logout();
        // ... other existing methods
    }
}