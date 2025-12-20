using System.Collections.ObjectModel;
using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class ConsultationDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IConsultationService _consultationService;

        // Master list to hold all data (for searching)
        private List<ConsultationQueueDto> _allPatients = new();

        private bool _isBusy;
        private string _searchText = string.Empty;

        // Collection bound to the UI (displays filtered results)
        public ObservableCollection<ConsultationQueueDto> PatientQueue { get; } = new();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Filter the list whenever text changes
                    FilterPatientList();
                }
            }
        }

        public ICommand SelectPatientCommand { get; }
        public ICommand RefreshCommand { get; }

        public ConsultationDetailsViewModel(IAuthService authService, IConsultationService consultationService)
        {
            _authService = authService;
            _consultationService = consultationService;

            // Navigation Command
            SelectPatientCommand = new Command<ConsultationQueueDto>(async (item) => await SelectPatientAsync(item));

            RefreshCommand = new Command(async () => await LoadQueueAsync());

            LoadQueueAsync();
        }

        public async Task LoadQueueAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null) return;

                // Fetch from Service
                var items = await _consultationService.GetConsultationQueueAsync(user.staff_ID, DateTime.Now);

                // Update Master List
                _allPatients.Clear();
                _allPatients.AddRange(items);

                // Apply current filter (or show all)
                FilterPatientList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load queue.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterPatientList()
        {
            PatientQueue.Clear();

            var query = SearchText?.ToLower() ?? "";

            var filteredItems = string.IsNullOrWhiteSpace(query)
                ? _allPatients
                : _allPatients.Where(p =>
                    p.PatientName.ToLower().Contains(query) ||
                    p.PatientIC.Contains(query));

            foreach (var item in filteredItems)
            {
                PatientQueue.Add(item);
            }
        }

        private async Task SelectPatientAsync(ConsultationQueueDto item)
        {
            if (item == null) return;

            // Navigate to EndConsultationDetailsPage with the AppointmentId
            await Shell.Current.GoToAsync($"EndConsultationDetails?AppointmentId={item.ConsultationId}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}