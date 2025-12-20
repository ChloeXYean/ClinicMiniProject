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

        public async Task<string> RegisterWalkInPatient(string fullName, string ic, string phone, string serviceType, bool isEmergency = false)
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

                // For emergency patients, prioritize getting the earliest available slot
                if (isEmergency)
                {
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

                    // If no slot available today, try to create one immediately for emergency
                    if (assignedSlot == null && doctors.Count > 0)
                    {
                        assignedDoctorId = doctors.First().staff_ID;
                        assignedSlot = DateTime.Now.AddMinutes(5); // Emergency slot in 5 minutes
                    }
                }
                else
                {
                    // Regular walk-in patients
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
                    status = isEmergency ? "Emergency" : "Pending",
                    service_type = serviceType
                };

                await _appointmentService.AddAppointmentAsync(appointment);

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
                .Where(a => a.status == "Pending")
                .OrderBy(a => a.appointedAt)
                .ToList();
        }

        public async Task<List<PatientQueueDto>> GetWalkInQueueForToday()
        {
            // 1. Fetch appointments for today that are "Pending", "Emergency", or "checked_in"
            // This depends on your service logic (e.g., _appointmentService)
            var today = DateTime.Today;
            var appointments = await _appointmentService.GetAppointmentsByDateAsync(today); // You might need to add this method to your Service if missing

            // 2. Filter for Walk-In only and prioritize emergency patients
            var walkIns = appointments
                .Where(a => a.status == "Pending" || a.status == "Emergency") // Include emergency status
                .ToList();

            // 3. Sort by priority: Emergency patients first, then by appointment time
            var sortedWalkIns = walkIns
                .OrderByDescending(a => a.status == "Emergency") // Emergency patients first
                .ThenBy(a => a.appointedAt) // Then by appointment time
                .ToList();

            // 4. Convert DB Model -> UI DTO
            var queueList = new List<PatientQueueDto>();

            foreach (var app in sortedWalkIns)
            {
                var patient = _patientService.GetPatientByIC(app.patient_IC);

                queueList.Add(new PatientQueueDto
                {
                    PatientName = patient?.patient_name ?? "Unknown",
                    QueueId = app.appointment_ID,
                    ICNumber = app.patient_IC,
                    RegisteredTime = app.appointedAt?.ToString("hh:mm tt") ?? "--",
                    PhoneNumber = patient?.patient_contact ?? "N/A",

                    // --- FETCH REAL REASON FROM APPOINTMENT ---
                    ServiceType = app.service_type ?? "General Consultation",
                    IsEmergency = app.status == "Emergency" // Add emergency status to DTO
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

