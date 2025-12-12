using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IAppointmentService
    {
        // TODO: Existing methods... maybe got another functions
        
        // Add these new methods
        Task<IEnumerable<Appointment>> GetAppointmentsByStaffAndDateRangeAsync(
            string staffId, 
            DateTime startDate, 
            DateTime endDate);
            
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string staffId);
        
        // determine patient type
        bool IsAppUser(string patientId);

        Task<Patient?> GetPatientByIcAsync(string patientIc);

        Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string nameQuery);

        Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);

        Task UpdateAppointmentAsync(Appointment appointment);

        Task<Patient?> GetRandomWalkInPatientAsync();
    }
}