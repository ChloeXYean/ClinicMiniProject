using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public class ReportingManagementViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IReportingService _reportingService;

        private DateTime _selectedDate = DateTime.Today;
        private ReportingSummaryDto? _summary;

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public ReportingSummaryDto? Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        public ICommand RefreshCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReportingManagementViewModel(IAuthService authService, IReportingService reportingService)
        {
            _authService = authService;
            _reportingService = reportingService;

            RefreshCommand = new Command(async () => await RefreshAsync());
        }

        public async Task RefreshAsync()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

            var start = SelectedDate.Date;
            var end = start.AddDays(1);

            Summary = await _reportingService.GetDoctorReportingAsync(doctor.staff_ID, start, end);
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

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
