using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public class ConsultationDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;

        private string _searchText = string.Empty;
        private ConsultationDetailsDto? _currentDetails;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ConsultationDetailsDto? CurrentDetails
        {
            get => _currentDetails;
            set => SetProperty(ref _currentDetails, value);
        }

        public ObservableCollection<PatientLookupDto> SearchResults { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand StartConsultationCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConsultationDetailsViewModel(IAuthService authService, IConsultationService consultationService)
        {
            _authService = authService;
            _consultationService = consultationService;

            RefreshCommand = new Command(async () => await RefreshAsync());
            SearchCommand = new Command(async () => await SearchAsync());
            StartConsultationCommand = new Command(async () => await StartAsync());
        }

        public async Task RefreshAsync()
        {
            var doctor = _authService.GetCurrentUser();
            if (doctor == null)
                return;

            CurrentDetails = await _consultationService.GetCurrentConsultationDetailsAsync(doctor.staff_ID, DateTime.Now);
        }

        public async Task SearchAsync()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            var results = await _consultationService.SearchPatientsByNameAsync(SearchText);
            foreach (var r in results)
                SearchResults.Add(r);
        }

        public async Task StartAsync()
        {
            if (CurrentDetails == null)
                return;

            await _consultationService.StartConsultationAsync(CurrentDetails.AppointmentId);
            await RefreshAsync();
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
