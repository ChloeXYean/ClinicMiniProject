using ClinicMiniProject.Models;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.Services;
using ClinicMiniProject.Dtos;


namespace ClinicMiniProject.Controller
{
    public class NurseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly PatientService _patientService;
        private readonly IStaffService _staffService;

        public NurseController(
            IAppointmentService appointmentService,
            PatientService patientService,
            IStaffService staffService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _staffService = staffService;
        }


        public async Task<List<Appointment>> ViewAppointmentList(DateTime selectedDate)
        {
            // filter by staff or role
            var start = selectedDate.Date;
            var end = start.AddDays(1);

            var appointments = await _appointmentService
                .GetAppointmentsByStaffAndDateRangeAsync("", start, end);

            return appointments.ToList();
        }

        public async Task<bool> RegisterWalkInPatient(string fullName,string ic,string phone, string? preferredDoctorId = null)
        { //Reason idk need or not 
            try
            {
                var patient = new Patient
                {
                    patient_IC = ic,
                    patient_name = fullName,
                    patient_contact = phone,
                    isAppUser = false
                };

                _patientService.AddPatient(patient);

                string doctorId = preferredDoctorId ?? "DOC001"; // TODO: real logic

                var slot = _appointmentService.AssignWalkInTimeSlot(doctorId,DateTime.Today);

                var appointment = new Appointment
                {
                    patient_IC = ic,
                    staff_ID = doctorId,
                    appointedAt = slot,
                    bookedAt = DateTime.Now,
                    status = slot.HasValue ? "Pending" : "NoSlot",
                    //reason = serviceType
                };

                _appointmentService.AddAppointment(appointment);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Appointment>> GetCompletedConsultationsAsync(string doctorId)
        {
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(doctorId);

            return appointments.Where(a => a.status == "Completed").OrderByDescending(a => a.appointedAt).ToList();
        }

        public Patient? ViewPatientDetails(string patientIC)
        {
            return _patientService.GetPatientByIC(patientIC);
        }

        public async Task<List<Appointment>> GetUpcomingAppointment()
        {
            var now = DateTime.Now;

            var appointments = await _appointmentService
                .GetAppointmentsByStaffAndDateRangeAsync(
                    staffId: "",          
                    startDate: now,
                    endDate: now.AddDays(30) 
                );

            return appointments
                .Where(a => a.status == "Pending" || a.status == "Scheduled")
                .OrderBy(a => a.appointedAt)
                .ToList();
        }

        public async Task<List<PatientQueueDto>> GetWalkInQueueForToday()
        {
            // 1. Fetch appointments for today that are "Pending" or "checked_in"
            // This depends on your service logic (e.g., _appointmentService)
            var today = DateTime.Today;
            var appointments = await _appointmentService.GetAppointmentsByDateAsync(today); // You might need to add this method to your Service if missing

            // 2. Filter for Walk-In only
            var walkIns = appointments
                .Where(a => a.status == "Pending") // Adjust status check as needed
                .ToList();

            // 3. Convert DB Model -> UI DTO
            var queueList = new List<PatientQueueDto>();

            foreach (var app in walkIns)
            {
                queueList.Add(new PatientQueueDto
                {
                    // MAPPING: Use the real DB columns here
                    PatientName = app.patient_IC, // Or fetch the name using PatientService if you only have IC
                    RegisteredTime = app.appointedAt?.ToString("hh:mm tt") ?? "--:--",

                    // QUEUE ID LOGIC:
                    // If you have a specific queue_number column, use it. 
                    // Otherwise, format the Appointment ID or create a fake one based on order.
                    QueueId = $"Q-{app.appointment_ID.Substring(0, 4)}",

                    ICNumber = app.patient_IC
                });
            }

            return queueList;
        }
    }
}
