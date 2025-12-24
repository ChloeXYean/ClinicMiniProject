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
        public async Task<bool> AddPatientAsync(Patient patient)
        {
            if (patient == null || string.IsNullOrWhiteSpace(patient.patient_IC))
                return false;

            try
            {
                var existing = await _context.Patients
                    .FirstOrDefaultAsync(p => p.patient_IC == patient.patient_IC);

                if (existing == null)
                {
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error adding patient: {ex.Message}");
                return false;
            }
        }

        // Get patient by IC
        public async Task<Patient?> GetPatientByICAsync(string patient_IC)
        {
            try
            {
                return await _context.Patients
                    .FirstOrDefaultAsync(p => p.patient_IC == patient_IC);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error getting patient by IC: {ex.Message}");
                return null;
            }
        }

        // Get all patients
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            try
            {
                return await _context.Patients.ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error getting all patients: {ex.Message}");
                return new List<Patient>();
            }
        }

        // Get patient appointments
        public async Task<List<Appointment>> GetPatientsAppointmentsAsync(string patient_IC)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error getting patient appointments: {ex.Message}");
                return new List<Appointment>();
            }
        }

        // Get upcoming appointment
        public async Task<Appointment?> GetUpcomingAppointmentEntityAsync(string patient_IC)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error getting upcoming appointment: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetConsultationQueueAsync(Appointment appt)
        {
            if (appt == null || !appt.appointedAt.HasValue) return 0;

            try
            {
                var date = appt.appointedAt.Value.Date;

                return await _context.Appointments
                    .CountAsync(a =>
                        a.staff_ID == appt.staff_ID &&
                        a.status == "Pending" &&
                        a.appointedAt.HasValue &&
                        a.appointedAt.Value.Date == date &&
                        a.appointedAt < appt.appointedAt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error getting consultation queue: {ex.Message}");
                return 0;
            }
        }

        // Update patient profile
        public async Task<bool> UpdatePatientProfileAsync(string ic, string name, string contact, string email)
        {
            try
            {
                var p = await _context.Patients.FirstOrDefaultAsync(x => x.patient_IC == ic);
                if (p == null) return false;

                p.patient_name = name;
                p.patient_contact = contact;
                p.patient_email = email;

                int result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientService] Error updating patient profile: {ex.Message}");
                return false;
            }
        }

        // Get profile
        public async Task<Patient?> GetPatientPersonalInformationAsync(string patient_IC)
        {
            return await GetPatientByICAsync(patient_IC);
        }
    }
}