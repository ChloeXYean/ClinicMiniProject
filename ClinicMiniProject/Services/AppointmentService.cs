using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class AppointmentService : IAppointmentService
    {
        // TODO: link with database (replace in-memory lists with real DB queries)
        private static readonly List<Appointment> _appointments = new();
        private static readonly List<Patient> _patients = new();

        private static readonly Random _rng = new();

        public AppointmentService()
        {
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffAndDateRangeAsync(
            string staffId, 
            DateTime startDate, 
            DateTime endDate)
        {
            await Task.Yield();
            return _appointments
                .Where(a => a.staff_ID == staffId && a.appointedAt >= startDate && a.appointedAt < endDate)
                .ToList();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string staffId)
        {
            await Task.Yield();
            var now = DateTime.Now;
            return _appointments
                .Where(a => a.staff_ID == staffId && a.appointedAt > now && (a.status == "Scheduled" || a.status == "Pending"))
                .OrderBy(a => a.appointedAt)
                .ToList();
        }

        public bool IsAppUser(string patientId)
        {
            // TODO: link with database
            return _patients.Any(p => p.patient_IC == patientId && p.isAppUser);
        }

        public async Task<Patient?> GetPatientByIcAsync(string patientIc)
        {
            // TODO: link with database
            await Task.Yield();
            return _patients.FirstOrDefault(p => p.patient_IC == patientIc);
        }

        public async Task<IEnumerable<Patient>> SearchPatientsByNameAsync(string nameQuery)
        {
            // TODO: link with database
            await Task.Yield();
            if (string.IsNullOrWhiteSpace(nameQuery))
                return Enumerable.Empty<Patient>();

            var q = nameQuery.Trim();
            return _patients
                .Where(p => !string.IsNullOrWhiteSpace(p.patient_name) && p.patient_name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
        {
            // TODO: link with database
            await Task.Yield();
            return _appointments.FirstOrDefault(a => a.appointmentID == appointmentId);
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            // TODO: link with database
            await Task.Yield();
            var idx = _appointments.FindIndex(a => a.appointmentID == appointment.appointmentID);
            if (idx >= 0)
            {
                _appointments[idx] = appointment;
                return;
            }

            _appointments.Add(appointment);
        }

        public async Task<Patient?> GetRandomWalkInPatientAsync()
        {
            // Walk-in patient = not an app user
            // TODO: link with database
            await Task.Yield();
            var walkIns = _patients.Where(p => !p.isAppUser).ToList();
            if (walkIns.Count == 0)
                return null;

            return walkIns[_rng.Next(walkIns.Count)];
        }
    }
}