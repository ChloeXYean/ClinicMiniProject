using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.ViewModels
{
    public sealed class OnlineMedicalInquiryViewModel : INotifyPropertyChanged
    {
        private readonly IInquiryService _inquiryService;
        private readonly IAuthService? _authService;
        private readonly IAppointmentService? _appointmentService;

        private string _searchQuery = string.Empty;
        private string _selectedStatus = "All";
        private DateTime? _selectedDate;
        private string _uploadedPhotoPath = string.Empty;
        private string _photoNotes = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OnlineMedicalInquiryViewModel(
            IInquiryService inquiryService,
            IAuthService authService,
            IAppointmentService appointmentService)
        {
            _inquiryService = inquiryService;
            _authService = authService;
            _appointmentService = appointmentService;

            FilterCommand = new Command(async () => await LoadAsync());
            ViewInquiryDetailsCommand = new Command<string>(OnViewDetails);
            NavigateToHomeCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
            NavigateToInquiryCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorInquiryHistory"));
            NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            UploadPhotoCommand = new Command(async () => await UploadPhotoAsync());
            ViewPhotosCommand = new Command(async () => await Shell.Current.GoToAsync("InquiryPhotos"));

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            SubmitInquiryCommand = new Command(async () => await SubmitInquiryAsync());

            _ = InitializePatientDataAsync();
            _ = LoadAsync();
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set 
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    _ = LoadAsync(); // Auto-filter when status changes
                }
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set 
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    _ = LoadAsync(); // Auto-filter when date changes
                }
            }
        }

        public string UploadedPhotoPath
        {
            get => _uploadedPhotoPath;
            set => SetProperty(ref _uploadedPhotoPath, value);
        }

        public bool HasPhoto => !string.IsNullOrEmpty(UploadedPhotoPath);

        public string PhotoNotes
        {
            get => _photoNotes;
            set => SetProperty(ref _photoNotes, value);
        }

        // --- Patient Inquiry Creation Properties ---
        private string _symptomDescription = string.Empty;
        public string SymptomDescription
        {
            get => _symptomDescription;
            set => SetProperty(ref _symptomDescription, value);
        }

        private StaffListItem? _selectedDoctor;
        public StaffListItem? SelectedDoctor
        {
            get => _selectedDoctor;
            set => SetProperty(ref _selectedDoctor, value);
        }

        public ObservableCollection<StaffListItem> Doctors { get; } = new();

        public ObservableCollection<InquiryListItemVm> PatientInquiryHistory { get; } = new();

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Replied" };

        public ObservableCollection<InquiryListItemVm> InquiryList { get; } = new();

        public ICommand BackCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ViewInquiryDetailsCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToInquiryCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand UploadPhotoCommand { get; }
        public ICommand ViewPhotosCommand { get; }
        public ICommand SubmitInquiryCommand { get; }

        public async Task LoadAsync()
        {
            InquiryList.Clear();

            var items = await _inquiryService.GetInquiriesAsync(SearchQuery);
            
            // Apply filters
            var filteredItems = items.Where(i =>
            {
                // Status filter
                if (SelectedStatus != "All" && !string.Equals(i.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase))
                    return false;
                
                // Date filter
                if (SelectedDate.HasValue && i.CreatedAt.Date != SelectedDate.Value.Date)
                    return false;
                
                return true;
            });

            foreach (var i in filteredItems)
            {
                InquiryList.Add(new InquiryListItemVm
                {
                    InquiryId = i.InquiryId,
                    PatientIc = i.PatientIc,
                    PatientName = i.PatientName,
                    SymptomSnippet = BuildSnippet(i.FullSymptomDescription),
                    Status = i.Status,
                    CreatedDate = i.CreatedAt
                });
            }

            // Load patient-specific history if current user is a patient
            // Load patient-specific history
            var patient = _authService?.GetCurrentPatient();
            if (patient != null)
            {
                await LoadPatientHistoryAsync(patient.patient_IC);
            }
            else
            {
                var staff = _authService?.GetCurrentUser();
                if (staff != null && !staff.isDoctor)
                {
                    // Fallback or nurse view if needed, though usually patients use InquiryHistory
                    await LoadPatientHistoryAsync(staff.staff_ID);
                }
            }
        }

        private async Task LoadPatientHistoryAsync(string patientIc)
        {
            PatientInquiryHistory.Clear();
            var items = await _inquiryService.GetInquiriesByPatientIcAsync(patientIc);
            foreach (var i in items)
            {
                PatientInquiryHistory.Add(new InquiryListItemVm
                {
                    InquiryId = i.InquiryId,
                    PatientIc = i.PatientIc,
                    PatientName = i.PatientName,
                    SymptomSnippet = BuildSnippet(i.FullSymptomDescription),
                    Status = i.Status,
                    CreatedDate = i.CreatedAt
                });
            }
        }

        private async Task InitializePatientDataAsync()
        {
            var patient = _authService?.GetCurrentPatient();
            string? patientIc = patient?.patient_IC;

            if (string.IsNullOrEmpty(patientIc))
            {
                var staff = _authService?.GetCurrentUser();
                if (staff != null && !staff.isDoctor)
                {
                    patientIc = staff.staff_ID;
                }
            }

            if (string.IsNullOrEmpty(patientIc)) return;

            // Load Doctors consulted by this patient
            if (_appointmentService != null)
            {
                var appointments = await _appointmentService.GetAppointmentsByPatientIcAsync(patientIc);
                var consultedDoctors = appointments
                    .Where(a => string.Equals((a.status ?? "").Trim(), "Completed", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Staff)
                    .Where(s => s != null)
                    .GroupBy(s => s.staff_ID)
                    .Select(g => g.First())
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Doctors.Clear();
                    foreach (var doc in consultedDoctors)
                    {
                        Doctors.Add(new StaffListItem { Id = doc.staff_ID, Name = doc.staff_name });
                    }
                });
            }
        }

        private async Task SubmitInquiryAsync()
        {
            if (SelectedDoctor == null)
            {
                await Shell.Current.DisplayAlert("Required", "Please select a doctor", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SymptomDescription))
            {
                await Shell.Current.DisplayAlert("Required", "Please describe your symptoms", "OK");
                return;
            }

            var patient = _authService?.GetCurrentPatient();
            string? ic = patient?.patient_IC;
            string? name = patient?.patient_name;

            if (string.IsNullOrEmpty(ic))
            {
                var staff = _authService?.GetCurrentUser();
                ic = staff?.staff_ID;
                name = staff?.staff_name;
            }

            if (string.IsNullOrEmpty(ic) || string.IsNullOrEmpty(name)) return;

            var newInquiry = new InquiryDto
            {
                InquiryId = $"INQ{DateTime.Now:yyyyMMddHHmmss}",
                PatientIc = ic,
                PatientName = name,
                FullSymptomDescription = SymptomDescription,
                Status = "Pending",
                CreatedAt = DateTime.Now
                // Image handling would go here if needed
            };

            var success = await _inquiryService.CreateInquiryAsync(newInquiry);
            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Inquiry submitted successfully", "OK");
                SymptomDescription = string.Empty;
                SelectedDoctor = null;
                await LoadAsync();
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to submit inquiry", "OK");
            }
        }

        private void OnViewDetails(string inquiryId)
        {
            if (string.IsNullOrWhiteSpace(inquiryId))
                return;

            Shell.Current.GoToAsync($"///InquiryDetails?inquiryId={Uri.EscapeDataString(inquiryId)}");
        }

        private static string BuildSnippet(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var t = text.Trim();
            return t.Length <= 60 ? t : t.Substring(0, 60) + "...";
        }

        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task UploadPhotoAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select photo for inquiry"
                });

                if (result != null)
                {
                    UploadedPhotoPath = result.FullPath;
                    await Shell.Current.DisplayAlert("Success", "Photo uploaded successfully", "OK");
                    
                    // Navigate to photos page
                    await Shell.Current.GoToAsync("InquiryPhotos");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to upload photo: {ex.Message}", "OK");
            }
        }
    }

    public sealed class InquiryListItemVm
    {
        public string InquiryId { get; init; } = string.Empty;
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public string SymptomSnippet { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedDate { get; init; }
        
        public string DoctorName { get; set; } = string.Empty; // For patient history view
        public string SymptomHistory => SymptomSnippet;

        public ICommand ViewDetailsCommand { get; } = new Command<InquiryListItemVm>(async (vm) => 
        {
             await Shell.Current.GoToAsync($"InquiryDetailsView?inquiryId={vm.InquiryId}");
        });
    }

    public sealed class StaffListItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public override string ToString() => Name;
    }
}
