using System;
using System.Collections.Generic;
using System.Linq;

public class PatientService
{
    private List<Patient> patients = new List<Patient>();
    private List<Appointment> appointments = new List<Appointment>();
    private StaffService staffService;

    public PatientService(StaffService staffService)
    {
        this.staffService = staffService;
    }

    //Add a new patient
    public void AddPatient(Patient patient)
    {
        if (patient == null || string.IsNullOrWhiteSpace(patient.patient_IC))
        {
            Console.WriteLine("Invalid patient data.");
            return;
        }

        patients.Add(patient);
    }

    //Get patient by IC
    public Patient GetPatientByIC(string patient_IC)
    {
        return patients.FirstOrDefault(p => p.patient_IC == patient_IC);
    }

    //Get all patients
    public List<Patient> GetAllPatients()
    {
        return patients;
    }

    // Add appointment
    public void AddAppointment(Appointment appt)
    {
        appointments.Add(appt);
    }

    //---------------------------------------------------------------
    /*get upcoming appointment
     - date 
     - time 
     - doctor 
     - doctor consultation
     - payment counter
     - medical pickup counter */

    public Appointment GetUpcomingAppointmentEntity(string patient_IC)
    {
        return appointments
            .Where(a => a.patient_IC == patient_IC && a.status == "Pending") // return when IC correct and status is pending
            .OrderBy(a => a.appointedAt)
            .FirstOrDefault();
    }

    public UpcomingAppointmentView GetUpcomingAppointmentDetails(string patient_IC)
    {
        var appt = GetUpcomingAppointmentEntity(patient_IC);
        if (appt == null)
            return null;

        var doctor = staffService.GetStaffByID(appt.staff_ID);

        int consultationQueue = GetConsultationQueue(appt);
        int paymentQueue = GetPaymentQueue(appt);
        int pickupQueue = GetPickupQueue(appt);

        var view = new UpcomingAppointmentView
        {
            Time = appt.appointedAt.ToString("HH:mm"),
            DoctorName = doctor?.staff_name ?? "Unknown",
            ConsultationQueueCount = consultationQueue
        };

        // If still waiting for consultation, other stages are pending
        if (consultationQueue > 0)
        {
            view.PaymentCounter = "Pending";
            view.PickupCounter = "Pending";
        }
        else
        {
            view.PaymentCounter = paymentQueue > 0
                ? $"{paymentQueue} ahead"
                : "Your turn";

            if (paymentQueue == 0)
            {
                view.PickupCounter = pickupQueue > 0
                    ? $"{pickupQueue} ahead"
                    : "Your turn";
            }
            else
            {
                view.PickupCounter = "Pending";
            }
        }

        return view;
    }

    private int GetConsultationQueue(Appointment appt)
    {
        return appointments.Count(a =>
            a.staff_ID == appt.staff_ID &&
            a.appointedAt < appt.appointedAt &&
            a.appointment_status == "Pending"
        );
    }

    private int GetPaymentQueue(Appointment appt)
    {
        return appointments.Count(a =>
            a.consultation_status == "Completed" &&
            a.appointedAt < appt.appointedAt &&
            a.appointment_status == "Pending"
        );
    }

    private int GetPickupQueue(Appointment appt)
    {
        return appointments.Count(a =>
            a.consultation_status == "Completed" &&
            a.payment_status == "Completed" &&
            a.appointedAt < appt.appointedAt &&
            a.appointment_status == "Pending"
        );
    }

}

    //get appointment booking


    //get appointment history

    //get online inquiry

}