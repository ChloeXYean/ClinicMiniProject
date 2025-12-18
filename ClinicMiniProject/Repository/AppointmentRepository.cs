using ClinicMiniProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;
        public AppointmentRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }   

        public List<Appointment> GetAppointmentsByDate(DateTime date)
        {
            return _context.Appointments.Where(a => a.appointedAt.HasValue && a.appointedAt.Value.Date == date).ToList();
        }
        public List<Appointment> GetAppointmentsById(string id)
        {
            return _context.Appointments.Where(a => a.appointment_ID == id).ToList();
        }
        public List<Appointment> GetAppointmentsByPatientIC(string patientIC)
        {
            return _context.Appointments.Where(a => a.patient_IC == patientIC).ToList();
        }
        public void AddAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            _context.SaveChanges();
        }

        public void UpdateAppointmentStatus(string appointmentId, string status)
        {
            var appoint = _context.Appointments.FirstOrDefault(a => a.appointment_ID == appointmentId);
            if (appoint != null)
            {
                appoint.status = status;
                _context.SaveChanges();
            }
        }

        public IQueryable<Appointment> GetQueryable()
        {
            return _context.Appointments.AsQueryable();


        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            // Calculate the full 24-hour range for the given date
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Appointments
                .Where(a => a.appointedAt >= startOfDay && a.appointedAt < endOfDay)
                .OrderBy(a => a.appointedAt)
                .ToListAsync();
        }
    }
}
