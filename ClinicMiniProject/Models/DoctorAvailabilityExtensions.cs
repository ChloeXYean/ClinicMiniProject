using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicMiniProject.Models
{
    public static class DoctorAvailabilityExtensions
    {
        public static bool IsAvailable(this ClinicMiniProject.Models.DoctorAvailability da, DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => da.Monday,
                DayOfWeek.Tuesday => da.Tuesday,
                DayOfWeek.Wednesday => da.Wednesday,
                DayOfWeek.Thursday => da.Thursday,
                DayOfWeek.Friday => da.Friday,
                DayOfWeek.Saturday => da.Saturday,
                DayOfWeek.Sunday => da.Sunday,
                _ => false
            };
        }
    }
}
