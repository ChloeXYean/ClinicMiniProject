using System;
using System.Collections.Generic;
using System.Linq;
using ClinicMiniProject.Models;
using ClinicMiniProject.Services;

public class PatientService
{
    private List<Patient> patients = new List<Patient>();
    private List<Appointment> appointments = new List<Appointment>();
    private StaffService staffService;
    private ClinicService clinicService;

    public PatientService(StaffService staffService, ClinicService clinicService)
    {
        this.staffService = staffService;
        this.clinicService = clinicService;
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
    public Patient? GetPatientByIC(string patient_IC)
    {
        return patients.FirstOrDefault(p => p.patient_IC == patient_IC);
    }

    //Get all patients
    public List<Patient> GetAllPatients()
    {
        return patients;
    }

    // Get all appointments
    public List<Appointment> GetPatientsAppointments(string patient_IC)
    {
        return clinicService.GetAllAppointments()
            .Where(a => a.patient_IC == patient_IC)
            .OrderBy(a => a.appointment_status == "Pending" ? 0: 
            a.appointment_status == "Completed" ? 1:
            a.appointment_status == "Cancelled" ? 2 : 3)
            .ThenBy(a => a.appointedBy)
            .FirstOrDefault());
    }

    // Add appointment


    //---------------------------------------------------------------
    /*get upcoming appointment
     - date 
     - time 
     - doctor 
     - doctor consultation
     - payment counter
     - medical pickup counter */

    public Appointment? GetUpcomingAppointmentEntity(string patient_IC)
    {
        return appointments
            .Where(a => a.patient_IC == patient_IC && a.status == "Pending") // return when IC correct and status is pending
            .OrderBy(a => a.appointedAt)
            .FirstOrDefault();
    }

    public UpcomingAppointmentView? GetUpcomingAppointmentDetails(string patient_IC)
    {
        var appt = GetUpcomingAppointmentEntity(patient_IC);
        if (appt == null)
            return null;

        var doctor = staffService.GetStaff(appt.staff_ID);

        int consultationQueue = clinicService.GetConsultationQueue(appt);
        int paymentQueue = clinicService.GetPaymentQueue(appt);
        int pickupQueue = clinicService.GetPickupQueue(appt);

        var view = new UpcomingAppointmentView
        {
            Time = appt.appointedAt?.ToString("HH:mm") ?? "--:--", //not sure about the format
            DoctorName = doctor?.staff_name ?? "Unknown",
            ConsultationQueueCount = consultationQueue
        };

        // If still waiting for consultation, other stages are pending
        if (consultationQueue > 0)
        {
            view.PaymentStatus = "Pending";
            view.PickupStatus = "Pending";
        }
        else
        {
            view.PaymentStatus = paymentQueue > 0
                ? $"{paymentQueue} ahead"
                : "Your turn";

            if (paymentQueue == 0)
            {
                view.PickupStatus = pickupQueue > 0
                    ? $"{pickupQueue} ahead"
                    : "Your turn";
            }
            else
            {
                view.PickupStatus = "Pending";
            }
        }

        return view;
    }

    //private int GetConsultationQueue(Appointment appt)
    //{
    //    return appointments.Count(a =>
    //        a.staff_ID == appt.staff_ID &&
    //        a.appointedAt < appt.appointedAt &&
    //        a.appointment_status == "Pending"
    //    );
    //}

    //private int GetPaymentQueue(Appointment appt)
    //{
    //    return appointments.Count(a =>
    //        a.consultation_status == "Completed" &&
    //        a.appointedAt < appt.appointedAt &&
    //        a.appointment_status == "Pending"
    //    );
    //}

    //private int GetPickupQueue(Appointment appt)
    //{
    //    return appointments.Count(a =>
    //        a.consultation_status == "Completed" &&
    //        a.payment_status == "Completed" &&
    //        a.appointedAt < appt.appointedAt &&
    //        a.appointment_status == "Pending"
    //    );
    //}

    // get profile information
    public Patient? GetPatientPersonalInformation(string patient_IC)
    {
        return patients.FirstOrDefault(p => p.patient_IC == patient_IC);
    }

    // appointment booking

    //get appointment history

    //get online inquiry

}