using Microsoft.Extensions.Logging;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI;
using ClinicMiniProject.ViewModels;
using ClinicMiniProject.Controller;
using ClinicMiniProject.UI.Nurse;
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
            builder.Services.AddScoped<PatientService>();

            builder.Services.AddScoped<NurseController>();
            //ViewModels
            builder.Services.AddTransient<DoctorDashboardViewModel>();
            builder.Services.AddTransient<ConsultationDetailsViewModel>();
            builder.Services.AddTransient<ConsultationSessionViewModel>();
            builder.Services.AddTransient<ReportingManagementViewModel>();
            builder.Services.AddTransient<DoctorProfileViewModel>();

            //Auth
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();

            //Nurse
            builder.Services.AddTransient<NurseHomePage>();    
            builder.Services.AddTransient<RegisterPatientPage>();

//#if DEBUG
//            builder.Logging.AddDebug();
//#endif

            return builder.Build();
        }
    }
}
