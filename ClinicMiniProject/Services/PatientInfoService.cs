using System;
using System.Threading.Tasks;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.Services
{
    public class PatientInfoService : IPatientInfoService
    {
        public PatientInfoService()
        {
        }

        public async Task<PatientInfoDto?> GetPatientInfoAsync(string patientIc)
        {
            await Task.Yield();

            // TODO: link with database
            // Required data to show on Patient Information page:
            // - Name (Patient.patient_name)
            // - Phone no (Patient.patient_contact)
            // - IC no (Patient.patient_IC)
            // - Service type (not present in current Patient model; likely from Appointment/Service table)
            // - Patient type (Online vs Walk-in) -> can be derived from Patient.isAppUser
            // - Registered time (not present in current Patient model; needs a createdAt/registeredAt field in DB)
            //
            // Since models don't include registered time / service type, return placeholders for now.

            return new PatientInfoDto
            {
                PatientIc = patientIc,
                Name = string.Empty,
                PhoneNo = string.Empty,
                ServiceType = string.Empty,
                PatientType = string.Empty,
                RegisteredTime = string.Empty
            };
        }
    }
}
