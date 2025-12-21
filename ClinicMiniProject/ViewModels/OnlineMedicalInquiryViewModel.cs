using ClinicMiniProject.Dtos;
using ClinicMiniProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // Required for LINQ extensions
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Models;

namespace ClinicMiniProject.ViewModels
{
    public sealed class OnlineMedicalInquiryViewModel : INotifyPropertyChanged
    {
        private readonly IInquiryService _inquiryService;
        private readonly IAuthService? _authService;
        private readonly IAppointmentService? _appointmentService;
        private readonly AppDbContext _context;

        private string _searchQuery = string.Empty;
        private string _selectedStatus = "All";
        private DateTime _filterDate = DateTime.Today;
        private bool _isDateFilterActive = false;
        private string _uploadedPhotoPath = string.Empty;
        private string _photoNotes = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OnlineMedicalInquiryViewModel(
                    IInquiryService inquiryService,
                    IAuthService authService,
                    IAppointmentService appointmentService,
                    AppDbContext context)
        {
            _inquiryService = inquiryService;
            _authService = authService;
            _appointmentService = appointmentService;
            _context = context;

            // Commands
            FilterCommand = new Command(async () => await LoadAsync());
            ViewInquiryDetailsCommand = new Command<string>(OnViewDetails);

            NavigateToHomeCommand = new Command(async () => await NavigateHomeAsync());
            NavigateToInquiryCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorInquiryHistory"));
            NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));

            UploadPhotoCommand = new Command(async () => await UploadPhotoAsync());
            ViewPhotosCommand = new Command(async () => await Shell.Current.GoToAsync("InquiryPhotos"));

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            SubmitInquiryCommand = new Command(async () => await SubmitInquiryAsync());

        }
        private async Task NavigateHomeAsync()
        {
            var user = _authService?.GetCurrentUser();
            if (user != null && !user.isDoctor)
            {
                // Is Nurse
                await Shell.Current.GoToAsync("///NurseHomePage");
            }
            else
            {
                // Default to Doctor (or Patient doesn't use this command usually)
                await Shell.Current.GoToAsync("///DoctorDashboardPage");
            }
        }

        #region Properties

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

        public DateTime FilterDate
        {
            get => _filterDate;
            set
            {
                if (SetProperty(ref _filterDate, value))
                {
                    _isDateFilterActive = true;
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

        // Collections
        public ObservableCollection<StaffListItem> Doctors { get; } = new();
        public ObservableCollection<InquiryListItemVm> PatientInquiryHistory { get; } = new();
        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Replied" };
        public ObservableCollection<InquiryListItemVm> InquiryList { get; } = new();

        #endregion

        #region Commands
        public ICommand BackCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ClearDateCommand { get; } // Declared but usually not initialized in snippet, can add if needed
        public ICommand ViewInquiryDetailsCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToInquiryCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand UploadPhotoCommand { get; }
        public ICommand ViewPhotosCommand { get; }
        public ICommand SubmitInquiryCommand { get; }
        #endregion

        #region Methods

        public async Task LoadAsync()
        {
            InquiryList.Clear();

            var currentUser = _authService?.GetCurrentUser();
            var currentPatient = _authService?.GetCurrentPatient();

            // 1. Staff Logic (Doctor or Nurse)
            if (currentUser != null)
            {
                IEnumerable<InquiryDto> items;

                if (currentUser != null && currentUser.isDoctor)
                {
                    items = await _inquiryService.GetInquiriesByDoctorAsync(currentUser.staff_ID, SearchQuery);
                    FilterAndAddItems(items);
                }
                else if (currentUser != null && !currentUser.isDoctor)
                {
                    try
                    {
                        var allInquiriesQuery = _context.Inquiries
                            .Include(i => i.Patient)
                            .AsQueryable();

                        if (!string.IsNullOrWhiteSpace(SearchQuery))
                        {
                            string q = SearchQuery.ToLower();
                            allInquiriesQuery = allInquiriesQuery.Where(i =>
                                (i.Patient != null && i.Patient.patient_name.ToLower().Contains(q)));
                        }

                        var allInquiries = await allInquiriesQuery
                                                   .OrderByDescending(i => i.AskDatetime)
                                                   .ToListAsync();

                        var dtos = new List<InquiryDto>();
                        foreach (var i in allInquiries)
                        {
                            dtos.Add(new InquiryDto
                            {
                                InquiryId = i.InquiryId, // Uses capitalized property from Model
                                PatientIc = i.PatientIc,
                                PatientName = i.Patient?.patient_name ?? "Unknown",
                                FullSymptomDescription = i.SymptomDescription, // Uses capitalized property
                                Status = i.Status,
                                CreatedAt = i.AskDatetime // FIXED: Map AskDatetime to CreatedAt
                            });
                        }

                        FilterAndAddItems(dtos);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading nurse inquiries: {ex.Message}");
                    }
                }
                // 3. PATIENT LOGIC
                else if (currentPatient != null)
                {
                    await LoadPatientHistoryAsync(currentPatient.patient_IC);
                }
            }
        }



        private void FilterAndAddItems(IEnumerable<InquiryDto> items)
        {
            var filteredItems = items.Where(i =>
            {
                // Status filter
                if (SelectedStatus != "All" && !string.Equals(i.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Date filter (only if active)
                if (_isDateFilterActive && i.CreatedAt.Date != FilterDate.Date)
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
                    DoctorName = i.DoctorName ?? "Unknown",
                    SymptomSnippet = BuildSnippet(i.FullSymptomDescription),
                    Status = i.Status,
                    CreatedDate = i.CreatedAt
                });
            }
        }

        public async Task InitializePatientDataAsync()
        {
            var patient = _authService?.GetCurrentPatient();
            string? patientIc = patient?.patient_IC;

            // Fallback for staff testing
            if (string.IsNullOrEmpty(patientIc))
            {
                var staff = _authService?.GetCurrentUser();
                if (staff != null && !staff.isDoctor) patientIc = staff.staff_ID;
            }

            if (string.IsNullOrEmpty(patientIc)) return;

            // Using _appointmentService (Now properly injected)
            if (_appointmentService != null)
            {
                // 1. Get History
                var appointments = await _appointmentService.GetAppointmentsByPatientIcAsync(patientIc);

                // 2. Filter: Must be "Completed" AND "isDoctor"
                var consultedDoctors = appointments
                    .Where(a => string.Equals((a.status ?? "").Trim(), "Completed", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Staff)
                    .Where(s => s != null && s.isDoctor)
                    .GroupBy(s => s.staff_ID) // Remove duplicates
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
            // 1. Validation
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

            // 2. Get Patient Info
            var patient = _authService?.GetCurrentPatient();
            string? ic = patient?.patient_IC;
            string? name = patient?.patient_name;

            // Fallback for staff testing
            if (string.IsNullOrEmpty(ic))
            {
                var staff = _authService?.GetCurrentUser();
                ic = staff?.staff_ID;
                name = staff?.staff_name;
            }

            if (string.IsNullOrEmpty(ic) || string.IsNullOrEmpty(name))
            {
                await Shell.Current.DisplayAlert("Error", "User information not found.", "OK");
                return;
            }

            // 3. Create DTO with DOCTOR ID & Auto-Generated ID
            var newInquiry = new InquiryDto
            {
                InquiryId = "I" + new Random().Next(100000, 999999).ToString(), // 7 chars
                PatientIc = ic,
                PatientName = name,
                FullSymptomDescription = SymptomDescription,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                DoctorId = SelectedDoctor.Id
            };

            try
            {
                var success = await _inquiryService.CreateInquiryAsync(newInquiry);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Your inquiry has been submitted successfully!", "OK");

                    // Reset form
                    SymptomDescription = string.Empty;
                    SelectedDoctor = null;
                    UploadedPhotoPath = string.Empty;
                    PhotoNotes = string.Empty;

                    // Navigate back
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to submit inquiry. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void OnViewDetails(string inquiryId)
        {
            if (!string.IsNullOrWhiteSpace(inquiryId))
                Shell.Current.GoToAsync($"///InquiryDetailsPage?inquiryId={Uri.EscapeDataString(inquiryId)}");
        }

        private static string BuildSnippet(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var t = text.Trim();
            return t.Length <= 60 ? t : t.Substring(0, 60) + "...";
        }

        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
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
                    await Shell.Current.GoToAsync("InquiryPhotos");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to upload photo: {ex.Message}", "OK");
            }
        }

        #endregion
    }

    public sealed class InquiryListItemVm
    {
        public string InquiryId { get; init; } = string.Empty;
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public string SymptomSnippet { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedDate { get; init; }

        public string DoctorName { get; set; } = string.Empty;
        public string SymptomHistory => SymptomSnippet;

        // Uses the instance property InquiryId, robust against binding mismatches
        public ICommand ViewDetailsCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync($"InquiryDetailsView?inquiryId={InquiryId}");
        });
    }

    public sealed class StaffListItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public override string ToString() => Name;
    }
}