using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Repository
{
    public interface IAppointmentRepository
    {
        List<Appointment> GetAppointmentsByDate(DateTime date);
        List<Appointment> GetAppointmentsById(string id);
        List<Appointment> GetAppointmentsByPatientIC(string patientIC);

        void AddAppointment(Appointment appointment);

        void UpdateAppointmentStatus(string appointmentId, string status);

        IQueryable<Appointment> GetQueryable();

        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
    }
}
