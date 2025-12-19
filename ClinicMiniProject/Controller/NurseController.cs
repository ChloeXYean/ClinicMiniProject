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
            var start = selectedDate.Date;
            var end = start.AddDays(1);

            var appointments = await _appointmentService
                .GetAppointmentsByStaffAndDateRangeAsync("", start, end);

            return appointments.ToList();
        }

        public async Task<string> RegisterWalkInPatient(string fullName, string ic, string phone, string serviceType)
        { 
            try
            {
                var existingPatient = _patientService.GetPatientByIC(ic);
                if (existingPatient == null)
                {
                    var patient = new Patient
                    {
                        patient_IC = ic,
                        patient_name = fullName,
                        patient_contact = phone,
                        isAppUser = false
                    };
                    _patientService.AddPatient(patient);
                }

                var doctors = _staffService.GetAllDocs();

                if (doctors == null || doctors.Count == 0)
                {
                    return "Unavailable: No doctors found in the system.";
                }

                string assignedDoctorId = null;
                DateTime? assignedSlot = null;

                foreach (var doc in doctors)
                {
                    var slot = _appointmentService.AssignWalkInTimeSlot(doc.staff_ID, DateTime.Today);

                    if (slot.HasValue)
                    {
                        assignedDoctorId = doc.staff_ID;
                        assignedSlot = slot;
                        break;
                    }
                }

                if (assignedDoctorId == null || assignedSlot == null)
                {
                    return "Unavailable: All doctors are fully booked today.";
                }

                var appointment = new Appointment
                {
                    patient_IC = ic,
                    staff_ID = assignedDoctorId,
                    appointedAt = assignedSlot,
                    bookedAt = DateTime.Now,
                    status = "Pending",
                    service_type = serviceType
                };

                _appointmentService.AddAppointment(appointment);

                return "Success";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
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

                    // QUEUE ID LOGIC:
                    // If you have a specific queue_number column, use it. 
                    // Otherwise, format the Appointment ID or create a fake one based on order.
                    QueueId = $"Q-{app.appointment_ID.Substring(0, 4)}",

                    ICNumber = app.patient_IC
                });
            }

            return queueList;
        }


        public async Task<bool> CompleteAppointment(string appointmentId)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

                if (appointment == null) return false;

                appointment.status = "Completed";

                await _appointmentService.UpdateAppointmentAsync(appointment);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Appointment>> GetAllAppointmentsHistory()
        {
            var start = DateTime.Now.AddMonths(-12);
            var end = DateTime.Now.AddMonths(6);

            var appointments = await _appointmentService.GetAppointmentsByStaffAndDateRangeAsync("", start, end);

            return appointments.OrderByDescending(x => x.appointedAt).ToList();
        }

    }


}

