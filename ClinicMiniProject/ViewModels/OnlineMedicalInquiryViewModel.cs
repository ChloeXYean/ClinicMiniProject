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

        private string _searchQuery = string.Empty;
        private string _selectedStatus = "All";
        private DateTime? _selectedDate;
        private string _uploadedPhotoPath = string.Empty;
        private string _photoNotes = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OnlineMedicalInquiryViewModel(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;

            FilterCommand = new Command(async () => await LoadAsync());
            ViewInquiryDetailsCommand = new Command<string>(OnViewDetails);
            NavigateToHomeCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorDashboardPage"));
            NavigateToInquiryCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorInquiryHistory"));
            NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///DoctorProfile"));
            UploadPhotoCommand = new Command(async () => await UploadPhotoAsync());
            ViewPhotosCommand = new Command(async () => await Shell.Current.GoToAsync("InquiryPhotos"));

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

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Replied" };

        public ObservableCollection<InquiryListItemVm> InquiryList { get; } = new();

        public ICommand FilterCommand { get; }
        public ICommand ViewInquiryDetailsCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToInquiryCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand UploadPhotoCommand { get; }
        public ICommand ViewPhotosCommand { get; }

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

                //// Date filter
                //if (SelectedDate.HasValue && i.CreatedAt.Date != SelectedDate.Value.Date)
                //    return false;

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
                    //CreatedDate = i.CreatedAt
                });
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
    }
}