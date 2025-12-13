using ClinicMiniProject.UI;
using ClinicMiniProject.UI.Doctor;

namespace ClinicMiniProject
{
    public partial class AppShell : Shell
    {
        bool _navigatedToLogin;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));

			Routing.RegisterRoute(nameof(DoctorDashboardPage), typeof(DoctorDashboardPage));
			Routing.RegisterRoute("AppointmentSchedule", typeof(AppointmentSchedulePage));
			Routing.RegisterRoute("ConsultationDetails", typeof(ConsultationDetailsPage));
			Routing.RegisterRoute("AppointmentHistory", typeof(AppointmentHistoryPage));
			Routing.RegisterRoute("ReportingManagement", typeof(ReportingManagementPage));
			Routing.RegisterRoute("Inquiry", typeof(InquiryPage));
			Routing.RegisterRoute("Profile", typeof(ProfilePage));
			Routing.RegisterRoute("PatientDetails", typeof(PatientDetailsPage));
			Routing.RegisterRoute("InquiryDetails", typeof(InquiryDetailsPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_navigatedToLogin) return;
            _navigatedToLogin = true;
            // Absolute or relative route depending on your shell hierarchy
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}
