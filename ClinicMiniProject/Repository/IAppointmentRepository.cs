using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Modules;

namespace ClinicMiniProject.Repository
{
    public interface IAppointmentRepository
    {
        List<Appointment> GetAppointmentsByDate(DateTime date);
        List<Appointment> GetAppointmentsByPatient(string patientIC);
        List<Appointment> GetAppointmentsById(int id);

        void UpdateAppointmentStatus(int appointmentId, string status);
    }
}
