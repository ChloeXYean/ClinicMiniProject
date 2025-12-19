using System.Threading.Tasks;
using ClinicMiniProject.Dtos;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IDoctorDashboardService
    {
        Task<DoctorDashboardDataDto> GetDashboardDataAsync(string doctorId);
    }
}