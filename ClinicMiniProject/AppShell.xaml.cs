using ClinicMiniProject.UI;
using ClinicMiniProject.UI.Doctor;
using ClinicMiniProject.UI.Nurse;
using ClinicMiniProject.UI.Patient;

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

            // --- Shared / Doctor Routes (Using simple string keys to match ViewModel) ---
            Routing.RegisterRoute("AppointmentSchedule", typeof(AppointmentSchedulePage));
            Routing.RegisterRoute("ConsultationDetails", typeof(ConsultationDetailsPage));
            Routing.RegisterRoute("AppointmentHistory", typeof(AppointmentHistoryPage));
            Routing.RegisterRoute("ReportingManagement", typeof(ReportingManagementPage));
            Routing.RegisterRoute("Inquiry", typeof(InquiryPage));       // Doctor/Nurse Inquiry View
            Routing.RegisterRoute("Profile", typeof(ProfilePage));       // Doctor Profile
            Routing.RegisterRoute("PatientDetails", typeof(PatientDetailsPage)); // Shared Patient Details
            Routing.RegisterRoute("InquiryDetails", typeof(InquiryDetailsPage));

            // --- Nurse Specific Routes ---
            Routing.RegisterRoute(nameof(EndConsultationPage), typeof(EndConsultationPage));
            Routing.RegisterRoute(nameof(RegisterPatientPage), typeof(RegisterPatientPage));
            Routing.RegisterRoute(nameof(WalkInPatientQueuePage), typeof(WalkInPatientQueuePage));

            // --- Patient Specific Routes ---
            Routing.RegisterRoute("InquiryHistory", typeof(InquiryHistory));
            Routing.RegisterRoute("InquiryDetailsView", typeof(InquiryHistory_DetailedView));
            Routing.RegisterRoute("OnlineInquiry", typeof(OnlineInquiryPatient));
            Routing.RegisterRoute("AppointmentHistory_NoHistory", typeof(AppointmentHistory_NoHistory));
            Routing.RegisterRoute("PatientConsultationDetails", typeof(PatientConsultationDetailsPage));
            Routing.RegisterRoute("PatientAppointmentHistory", typeof(PatientAppointmentHistoryPage));
            Routing.RegisterRoute("BookAnAppointment", typeof(AppointmentBooking_Patient));
            Routing.RegisterRoute("AppointmentBooking", typeof(AppointmentBooking_Patient));
            Routing.RegisterRoute("SelectDoctorPage", typeof(SelectDoctorPage));


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