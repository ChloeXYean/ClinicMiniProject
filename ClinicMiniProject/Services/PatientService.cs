using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class PatientService
    {
        private readonly AppDbContext _context;
        private readonly IAppointmentService _appointmentService;

        public PatientService(AppDbContext appDbContext, IAppointmentService appointmentService)
        {
            _context = appDbContext;
            _appointmentService = appointmentService;
        }

        // Add patient
        public void AddPatient(Patient patient)
        {
            if (patient == null || string.IsNullOrWhiteSpace(patient.patient_IC))
                return;

            var existing = _context.Patients
                .FirstOrDefault(p => p.patient_IC == patient.patient_IC);

            if (existing == null)
            {
                _context.Patients.Add(patient);
                _context.SaveChanges();
            }
        }

        // Get patient by IC
        public Patient? GetPatientByIC(string patient_IC)
        {
            return _context.Patients
                .FirstOrDefault(p => p.patient_IC == patient_IC);
        }

        // Get all patients
        public List<Patient> GetAllPatients()
        {
            return _context.Patients.ToList();
        }

        // Get patient appointments
        public async Task<List<Appointment>> GetPatientsAppointmentsAsync(string patient_IC)
        {
            // Auto-cancel expired appointments
            await _appointmentService.AutoCancelExpiredAppointmentsAsync();

            return await _context.Appointments
                .Where(a => a.patient_IC == patient_IC)
                .OrderBy(a => a.status == "Pending" ? 0 :
                              a.status == "Completed" ? 1 :
                              a.status == "Cancelled" ? 2 : 3)
                .ThenBy(a => a.appointedAt)
                .ToListAsync();
        }

        // Get upcoming appointment
        public async Task<Appointment?> GetUpcomingAppointmentEntityAsync(string patient_IC)
        {
            // Auto-cancel expired appointments
            await _appointmentService.AutoCancelExpiredAppointmentsAsync();
            System.Diagnostics.Debug.WriteLine($"[PatientService] Fetching upcoming appointment for IC: {patient_IC}");

            var now = DateTime.Now;
            var appointment = await _context.Appointments
                .Include(a => a.Staff)
                .Where(a => a.patient_IC == patient_IC &&
                            a.status == "Pending" &&
                            a.appointedAt >= DateTime.Today)
                .OrderBy(a => a.appointedAt)
                .FirstOrDefaultAsync();

            if (appointment != null)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Found appointment ID: {appointment.appointment_ID}, Date: {appointment.appointedAt}, Status: {appointment.status}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] No upcoming appointment found for IC: {patient_IC}");
            }

            return appointment;
        }

        public async Task<int> GetConsultationQueueAsync(Appointment appt)
        {
            if (appt == null || !appt.appointedAt.HasValue) return 0;

            var date = appt.appointedAt.Value.Date;

            return await _context.Appointments
                .CountAsync(a =>
                    a.staff_ID == appt.staff_ID &&
                    a.status == "Pending" &&
                    a.appointedAt.HasValue &&
                    a.appointedAt.Value.Date == date &&
                    a.appointedAt < appt.appointedAt);
        }

        // Update patient profile
        public async Task<bool> UpdatePatientProfileAsync(string ic, string name, string contact, string email)
        {
            var p = await _context.Patients.FirstOrDefaultAsync(x => x.patient_IC == ic);
            if (p == null) return false;

            p.patient_name = name;
            p.patient_contact = contact;
            p.patient_email = email;

            int result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // Get profile
        public Patient? GetPatientPersonalInformation(string patient_IC)
        {
            return _context.Patients
                .FirstOrDefault(p => p.patient_IC == patient_IC);
        }
    }
}