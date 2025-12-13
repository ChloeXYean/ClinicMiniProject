using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.Repository
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context; 

        public StaffRepository(AppDbContext appDbContext) 
        {
            _context = appDbContext;
        }

        public Staff GetStaffById(string id)
        {
            return _context.Staffs.FirstOrDefault(s => s.staff_ID == id);
        }

        public List<Staff> GetAllStaffs()
        {
            return _context.Staffs.ToList();
        }

        public void AddStaff(Staff staff)
        {
            _context.Staffs.Add(staff);
            _context.SaveChanges();
        }

        public void UpdateStaff(Staff staff)
        {
            _context.Staffs.Update(staff);
            _context.SaveChanges();
        }
    }
}
