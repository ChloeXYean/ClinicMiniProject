using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        public AppointmentRepository()
        {
            // Constructor implementation (if needed)   
        }

        public List<Appointment> GetAppointmentsByDate(DateTime date)
        {
            throw new NotImplementedException(); // Implement data retrieval logic here
        }

        public List<Appointment> GetAppointmentsByPatient(string patientIC)
        {
            throw new NotImplementedException();
        }

        public List<Appointment> GetAppointmentsById(int id)
        {
            throw new NotImplementedException();
        }

        public void UpdateAppointmentStatus(int appointmentId, string status)
        {
            throw new NotImplementedException();
        }


    }
}
