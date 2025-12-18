using Microsoft.Extensions.Logging;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.Controller;
using ClinicMiniProject.UI.Nurse;
using ClinicMiniProject.UI.Doctor;
using ClinicMiniProject.UI.Patient;
using ClinicMiniProject.Repository;

namespace ClinicMiniProject
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            //Register database context
            builder.Services.AddDbContext<AppDbContext>();

            //Register repositories
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();

            //Register services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
            //Original above two is AddSingleton
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IConsultationService, ConsultationService>();
            builder.Services.AddScoped<IAppointmentScheduleService, AppointmentScheduleService>();
            builder.Services.AddScoped<IPatientInfoService, PatientInfoService>();
            builder.Services.AddScoped<IReportingService, ReportingService>();
            builder.Services.AddScoped<IDoctorProfileService, DoctorProfileService>();
            builder.Services.AddScoped<INurseProfileService, NurseProfileService>();
            builder.Services.AddScoped<IStaffService, StaffService>();
            builder.Services.AddScoped<IInquiryService, InquiryService>();
            builder.Services.AddScoped<PatientService>();

            builder.Services.AddScoped<NurseController>();

            //ViewModels

            //Auth
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();

            //Staff
            builder.Services.AddTransient<DoctorDashboardPage>();
            builder.Services.AddTransient<DoctorDashboardViewModel>();

            builder.Services.AddTransient<ConsultationDetailsPage>();
            builder.Services.AddTransient<ConsultationDetailsViewModel>();

            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<DoctorProfileViewModel>();

            builder.Services.AddTransient<ReportingManagementPage>();
            builder.Services.AddTransient<ReportingManagementViewModel>();

            builder.Services.AddTransient<NurseHomePage>();
            builder.Services.AddTransient<NurseHomeViewModel>();

            builder.Services.AddTransient<NurseProfilePage>();
            builder.Services.AddTransient<NurseProfileViewModel>();

            builder.Services.AddTransient<RegisterPatientPage>();
            builder.Services.AddTransient<RegisterPatientViewModel>();

            builder.Services.AddTransient<PatientDetailsPage>();
            builder.Services.AddTransient<PatientDetailsViewModel>();

            builder.Services.AddTransient<EndConsultationPage>();
            builder.Services.AddTransient<EndConsultationViewModel>();

            builder.Services.AddTransient<WalkInPatientQueuePage>();
            builder.Services.AddTransient<WalkInPatientQueueViewModel>();

            builder.Services.AddTransient<AppointmentSchedulePage>();
            builder.Services.AddTransient<AppointmentScheduleViewModel>();

            builder.Services.AddTransient<AppointmentHistoryPage>();
            
            builder.Services.AddTransient<InquiryPage>();
            builder.Services.AddTransient<OnlineMedicalInquiryViewModel>();
            
            builder.Services.AddTransient<InquiryDetailsPage>();
            builder.Services.AddTransient<InquiryDetailsViewModel>();

            // Patient Pages
            builder.Services.AddTransient<PatientHomePage>();
            builder.Services.AddTransient<PatientHomeViewModel>();

            builder.Services.AddTransient<InquiryHistory>();
            builder.Services.AddTransient<InquiryHistory_DetailedView>();
            builder.Services.AddTransient<OnlineInquiryPatient>();
            builder.Services.AddTransient<PatientConsultationDetailsPage>();
            builder.Services.AddTransient<PatientAppointmentHistoryPage>();
            builder.Services.AddScoped<PatientAppointmentHistoryViewModel>();
            builder.Services.AddTransient<AppointmentBooking_Patient>();
            builder.Services.AddTransient<SelectDoctorPage>();
            builder.Services.AddTransient<SelectDoctorViewModel>();

            builder.Services.AddTransient<BookAnAppointmentViewModel>();
            builder.Services.AddTransient<PatientAppointmentBookingViewModel>();
            builder.Services.AddTransient<PatientConsultationDetailsViewModel>();

            return builder.Build();
        }
    }
}
