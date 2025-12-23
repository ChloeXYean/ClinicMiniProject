using ClinicMiniProject.UI;
using ClinicMiniProject.UI.Doctor;
using ClinicMiniProject.UI.Nurse;
using ClinicMiniProject.UI.Patient;
using Microsoft.Maui.Controls;
using System;

namespace ClinicMiniProject
{
    public partial class AppShell : Shell
    {
        bool _navigatedToLogin;

        public AppShell()
        {
            InitializeComponent();

            // --- Authentication ---
            // Root routes defined in AppShell.xaml don't need RegisterRoute
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));

            // --- Shared / Doctor Routes (Using simple string keys to match ViewModel) ---
            Routing.RegisterRoute(nameof(AppointmentSchedulePage), typeof(AppointmentSchedulePage));
            Routing.RegisterRoute("ConsultationDetails", typeof(ConsultationDetailsPage));
            Routing.RegisterRoute("ActiveConsultation", typeof(ActiveConsultationPage));
            Routing.RegisterRoute(nameof(AppointmentHistoryPage), typeof(AppointmentHistoryPage));
            Routing.RegisterRoute("ReportingManagement", typeof(ReportingManagementPage));
            Routing.RegisterRoute("Inquiry", typeof(InquiryPage));       // Doctor/Nurse Inquiry View
            Routing.RegisterRoute("DoctorInquiryHistory", typeof(InquiryHistoryPage)); // Doctor Inquiry History
            Routing.RegisterRoute("DoctorProfile", typeof(ProfilePage)); // Doctor Profile
            Routing.RegisterRoute("EditDoctorProfile", typeof(EditDoctorProfilePage)); // Edit Doctor Profile
            Routing.RegisterRoute("PatientDetails", typeof(PatientDetailsPage)); // Shared Patient Details
            Routing.RegisterRoute("InquiryDetails", typeof(InquiryDetailsPage));
            Routing.RegisterRoute("EndConsultationDetails", typeof(EndConsultationDetailsPage));
            Routing.RegisterRoute("EditNurseProfile", typeof(EditNurseProfilePage));

            Routing.RegisterRoute("NurseAppointmentHistory", typeof(AppointmentHistoryPage));
            // --- Nurse Specific Routes ---
            Routing.RegisterRoute(nameof(EndConsultationPage), typeof(EndConsultationPage));
            Routing.RegisterRoute(nameof(RegisterPatientPage), typeof(RegisterPatientPage));
            Routing.RegisterRoute(nameof(WalkInPatientQueuePage), typeof(WalkInPatientQueuePage));
            Routing.RegisterRoute("EditNurseProfile", typeof(EditNurseProfilePage));

            // --- Patient Specific Routes ---
            Routing.RegisterRoute("InquiryHistory", typeof(InquiryHistory));
            Routing.RegisterRoute("InquiryDetailsView", typeof(InquiryHistory_DetailedView));
            Routing.RegisterRoute("OnlineInquiry", typeof(OnlineInquiryPatient));
            Routing.RegisterRoute("PatientAppointmentHistory", typeof(PatientAppointmentHistoryPage));
            Routing.RegisterRoute("BookAnAppointment", typeof(AppointmentBooking_Patient));
            Routing.RegisterRoute("AppointmentBooking", typeof(AppointmentBooking_Patient));
            Routing.RegisterRoute("SelectDoctorPage", typeof(SelectDoctorPage));
            Routing.RegisterRoute("AppointmentCancellation", typeof(AppointmentCancellationPage));
            Routing.RegisterRoute("EditPatientProfile", typeof(EditPatientProfilePage));


            //Prevent lose 
            Routing.RegisterRoute("NurseReporting", typeof(ReportingManagementPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_navigatedToLogin) return;
            _navigatedToLogin = true;
            await Shell.Current.GoToAsync($"///{nameof(LoginPage)}");
        }
    }
}