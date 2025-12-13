using ClinicMiniProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services
{
    public interface IStaffService
    {
        public string GetStaffIdByName(string name);
        Staff GetStaff(string id);
        List<Staff> GetAllStaffs();
        void CreateStaff(Staff staff);
        void ModifyStaff(Staff staff);
        void DeleteStaff(Staff staff);
        void UpdateStaff(Staff staff);

        List<Appointment> ViewAppointmentList(DateTime selectedDate);
        List<Appointment> ViewAppointmentHistory(Patient patient);
        void UpdateAppointmentStatus(int appointmentId, string status);
    }
}
