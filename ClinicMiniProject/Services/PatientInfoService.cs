using ClinicMiniProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ClinicMiniProject.Services
{
    public class PatientInfoService : IPatientInfoService
    {
        private readonly AppDbContext _context;
        public PatientInfoService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<PatientInfoDto?> GetPatientInfoAsync(string patientIc)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.patient_IC == patientIc);

            if (patient == null) return null;

            // Note: If you have a 'ServiceType' or 'RegisteredTime' in your DB schema, map them here.

            return new PatientInfoDto
            {
                PatientIc = patient.patient_IC,
                Name = patient.patient_name,
                PhoneNo = patient.patient_contact ?? "N/A",
                // Logic: if isAppUser is true, they likely registered online
                PatientType = patient.isAppUser ? false : true,
                ServiceType = "General Checkup", 
                RegisteredTime = "N/A" 
            };
        }
    }
}
