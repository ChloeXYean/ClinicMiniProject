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

        public Task AddAppointmentAsync(Appointment appt);

        public DateTime? AssignWalkInTimeSlot(string doctorId, DateTime preferredDate, int workStartHour = 9, int workEndHour = 17, TimeSpan slotDuration = default);
        Task<IEnumerable<Appointment>> GetAppointmentsByStaffAndDateRangeAsync(
            string? staffId, 
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

        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);

        Task<IEnumerable<Appointment>> GetAppointmentsByPatientIcAsync(string patientIc);
    }
}