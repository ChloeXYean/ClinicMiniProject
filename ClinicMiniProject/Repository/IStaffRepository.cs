using ClinicMiniProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Repository
{
    public interface IStaffRepository
    {
        Staff GetStaffById(int id);
        List<Staff> GetAllStaffs();
        void AddStaff(Staff staff);
        void UpdateStaff(Staff staff);

    }
}
