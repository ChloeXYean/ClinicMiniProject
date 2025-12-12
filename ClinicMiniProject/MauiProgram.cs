using Microsoft.Extensions.Logging;
using ClinicMiniProject.Services;
using ClinicMiniProject.Services.Interfaces;
using ClinicMiniProject.UI;
using ClinicMiniProject.ViewModels;

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
                builder.Services.AddSingleton<IAuthService, AuthService>();
                builder.Services.AddSingleton<IDoctorDashboardService, DoctorDashboardService>();
                builder.Services.AddTransient<DoctorDashboardViewModel>();
                builder.Services.AddScoped<IAppointmentService, AppointmentService>();
                builder.Services.AddScoped<IConsultationService, ConsultationService>();
                builder.Services.AddTransient<ConsultationDetailsViewModel>();
                builder.Services.AddTransient<ConsultationSessionViewModel>();
                builder.Services.AddScoped<IAppointmentScheduleService, AppointmentScheduleService>();
                builder.Services.AddScoped<IPatientInfoService, PatientInfoService>();
                builder.Services.AddScoped<IReportingService, ReportingService>();
                builder.Services.AddTransient<ReportingManagementViewModel>();
                builder.Services.AddScoped<IDoctorProfileService, DoctorProfileService>();
                builder.Services.AddTransient<DoctorProfileViewModel>();

                builder.Services.AddTransient<LoginPage>();
                builder.Services.AddTransient<RegisterPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
