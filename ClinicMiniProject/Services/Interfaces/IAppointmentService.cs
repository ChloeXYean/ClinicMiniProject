using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Services.Interfaces
{
    public interface IAppointmentService
    {

        Task<bool> RescheduleAppointmentAsync(string appointmentId, DateTime newDate);
        public Task<bool> AddAppointmentAsync(Appointment appt);

        public Task<bool> HasConflictingAppointmentAsync(string patientIc, string doctorId, DateTime appointmentTime);

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

        // New methods for synchronized appointment booking
        Task<List<TimeSpan>> GetBookedTimeSlotsAsync(string doctorId, DateTime date);

        Task<bool> PatientHasAppointmentAtTimeAsync(string patientIc, DateTime appointmentTime);

        Task<List<TimeSpan>> GetPatientBookedTimeSlotsForDateAsync(string patientIc, DateTime date);

        Task<bool> IsAnyDoctorAvailableAtTimeAsync(DateTime appointmentDateTime);

        Task<List<TimeSpan>> GetUnavailableTimeSlotsForDateAsync(DateTime date);
        Task AutoCancelExpiredAppointmentsAsync();
        Task<bool> CancelAppointmentAsync(string appointmentId);
    }
}