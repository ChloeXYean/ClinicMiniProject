using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.ViewModels
{
    [QueryProperty(nameof(UserType), "UserType")]
    public class ReportingManagementViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IReportingService _reportingService;
        private readonly IInquiryService _inquiryService;

        private DateTime _selectedDate = DateTime.Today;
        private string _reportPeriodType = "Day";
        private string _selectedServiceType = "General Consultation";

        private string _userType = "Doctor";

        private ReportingSummaryDto? _summary;
        private int _totalOnlineInquiries;
        private int _repliedInquiries;
        private bool _hasReport;

        // --- PROPERTIES ---
        public DateTime ReportSelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public string ReportPeriodType
        {
            get => _reportPeriodType;
            set => SetProperty(ref _reportPeriodType, value);
        }

        public ObservableCollection<string> AvailableServiceTypes { get; } = new()
        {
            "General Consultation",
            "Follow Up Treatment",
            "Test Result Discussion",
            "Vaccination/Injection",
            "Medical Screening"
        };

        public string UserType
        {
            get => _userType;
            set
            {
                if (SetProperty(ref _userType, value))
                {
                    //Auto-refresh when user type changes
                    Task.Run(GenerateAsync); 
                }
            }
        }

        public string SelectedServiceType
        {
            get => _selectedServiceType;
            set => SetProperty(ref _selectedServiceType, value);
        }

        public ReportingSummaryDto? Summary
        {
            get => _summary;
            set
            {
                if (SetProperty(ref _summary, value))
                {
                    OnPropertyChanged(nameof(ReportDateRange));
                    OnPropertyChanged(nameof(ReportTimePeriod));
                    OnPropertyChanged(nameof(TotalConsultedCount));
                    OnPropertyChanged(nameof(WalkInConsultationCount));
                    OnPropertyChanged(nameof(OnlineConsultationCount));
                    OnPropertyChanged(nameof(GeneralConsultationCount));
                    OnPropertyChanged(nameof(FollowUpTreatmentCount));
                    OnPropertyChanged(nameof(TestResultDiscussionCount));
                    OnPropertyChanged(nameof(VaccinationInjectionCount));
                    OnPropertyChanged(nameof(TotalMedicalScreeningCount));
                    OnPropertyChanged(nameof(BloodTestCount));
                    OnPropertyChanged(nameof(BloodPressureCount));
                    OnPropertyChanged(nameof(SugarTestCount));
                }
            }
        }

        public bool HasReport
        {
            get => _hasReport;
            set => SetProperty(ref _hasReport, value);
        }

        // --- DISPLAY HELPERS ---
        public string ReportDateRange => Summary == null ? string.Empty : $"Date: {Summary.Start:dd MMM yyyy} - {Summary.End.AddDays(-1):dd MMM yyyy}";
        public string ReportTimePeriod => $"Time Period: {ReportPeriodType}";

        public int TotalConsultedCount => Summary?.TotalConsulted ?? 0;
        public int WalkInConsultationCount => Summary?.WalkInConsultation ?? 0;
        public int OnlineConsultationCount => Summary?.OnlineConsultation ?? 0;

        public int GeneralConsultationCount => Summary?.ServiceType.GeneralConsultation ?? 0;
        public int FollowUpTreatmentCount => Summary?.ServiceType.FollowUpTreatment ?? 0;
        public int TestResultDiscussionCount => Summary?.ServiceType.TestResultDiscussion ?? 0;
        public int VaccinationInjectionCount => Summary?.ServiceType.VaccinationOrInjection ?? 0;

        public int TotalMedicalScreeningCount => (Summary?.ServiceType.TotalMedicalScreening.BloodTest ?? 0)
                                              + (Summary?.ServiceType.TotalMedicalScreening.BloodPressure ?? 0)
                                              + (Summary?.ServiceType.TotalMedicalScreening.SugarTest ?? 0);

        public int BloodTestCount => Summary?.ServiceType.TotalMedicalScreening.BloodTest ?? 0;
        public int BloodPressureCount => Summary?.ServiceType.TotalMedicalScreening.BloodPressure ?? 0;
        public int SugarTestCount => Summary?.ServiceType.TotalMedicalScreening.SugarTest ?? 0;

        public int TotalOnlineInquiries
        {
            get => _totalOnlineInquiries;
            private set => SetProperty(ref _totalOnlineInquiries, value);
        }

        public int RepliedInquiriesCount
        {
            get => _repliedInquiries;
            private set => SetProperty(ref _repliedInquiries, value);
        }

        public ICommand GenerateReportCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReportingManagementViewModel(IAuthService authService, IReportingService reportingService, IInquiryService inquiryService)
        {
            _authService = authService;
            _reportingService = reportingService;
            _inquiryService = inquiryService;

            GenerateReportCommand = new Command(async () => await GenerateAsync());
        }

        public async Task GenerateAsync()
        {
            var start = ReportSelectedDate.Date;
            DateTime end;

            if (string.Equals(ReportPeriodType, "Week", StringComparison.OrdinalIgnoreCase))
                end = start.AddDays(7);
            else if (string.Equals(ReportPeriodType, "Month", StringComparison.OrdinalIgnoreCase))
                end = start.AddMonths(1);
            else
                end = start.AddDays(1); // Default to Day

           
            string? doctorId = null;

            if (UserType == "Nurse")
            {
                doctorId = "";
            }
            else
            {
                // DOCTOR: Only get my own appointments
                var doctor = _authService.GetCurrentUser();
                if (doctor == null) return;
                doctorId = doctor.staff_ID;
            }

            Summary = await _reportingService.GetDoctorReportingAsync(doctorId, start, end);

            var inquiries = await _inquiryService.GetInquiriesAsync(null);

            TotalOnlineInquiries = inquiries.Count;
            RepliedInquiriesCount = inquiries.Count(i => string.Equals(i.Status, "Replied", StringComparison.OrdinalIgnoreCase));

            HasReport = true;
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}