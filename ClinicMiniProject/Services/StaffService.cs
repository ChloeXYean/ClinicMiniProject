using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Modules;

namespace ClinicMiniProject.Services
{

    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IAppointmentReposirotu _aptRepo;

        public StaffService(IStaffRepository staffRepository, IAppointmentReposirotu aptRepo)
        {
            _staffRepository = staffRepository;
            _aptRepo = aptRepo;
        }

        public Staff GetStaff(int id)
        {
            return _staffRepository.GetStaffById(id);
        }

        public List<Staff> GetAllStaffs()
        {
            return _staffRepository.GetAllStaffs();
        }

        public void CreateStaff(Staff staff)
        {
            _staffRepository.AddStaff(staff);
        }

        public void ModifyStaff(Staff staff)
        {
            _staffRepository.UpdateStaff(staff);
        }

        public void DeleteStaff(Staff staff)
        {
            // Implementation for deleting staff (not in repository interface)
            throw new NotImplementedException();
        }

        public void UpdateStaff(Staff staff)
        {
            _staffRepository.UpdateStaff(staff);
        }

        public List<Appointment> ViewAppointmentList(DateTime selectedDate)
        {
            return _aptRepo.GetAppointmentsByDate(selectedDate);
        }

        public List<Appointment> ViewAppointmentHistory(Patient patient)
        {
            return _aptRepo.GetAppointmentsByPatient(patient.patientIC);
        }

        public void UpdateAppointmentStatus(int appointmentId, string status)
        {
            var appointment = _aptRepo.GetAppointmentById(appointmentId);
            if (appointment != null)
            {
                appointment.status = status;
                _aptRepo.UpdateAppointment(appointment);
            }
        }
    }
}
