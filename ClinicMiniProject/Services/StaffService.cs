using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;
using ClinicMiniProject.Repository;


namespace ClinicMiniProject.Services
{

    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IAppointmentRepository _aptRepo;

        public StaffService(IStaffRepository staffRepository, IAppointmentRepository aptRepo)
        {
            _staffRepository = staffRepository;
            _aptRepo = aptRepo;
        }
        public string GetStaffIdByName(string name)
        {
                var staff = _staffRepository.GetAllStaffs().FirstOrDefault(s =>s.staff_name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return staff?.staff_ID ?? string.Empty;
        }

        public Staff GetStaff(string id)
        {
            return _staffRepository.GetStaffById(id);
        }

        public List<Staff> GetAllStaffs()
        {
            return _staffRepository.GetAllStaffs();
        }
        public List<Staff> GetAllDocs()
        {
                return _staffRepository
                            .GetAllStaffs()
                            .Where(s => s.isDoctor == true)
                            .ToList();
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
            return _aptRepo.GetAppointmentsByPatientIC(patient.patient_IC);
            }
        
    }
}