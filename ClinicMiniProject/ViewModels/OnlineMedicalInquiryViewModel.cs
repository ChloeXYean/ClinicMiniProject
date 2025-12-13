using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public sealed class OnlineMedicalInquiryViewModel : INotifyPropertyChanged
    {
        private readonly IInquiryService _inquiryService;

        private string _searchQuery = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OnlineMedicalInquiryViewModel(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;

            FilterCommand = new Command(async () => await LoadAsync());
            ViewInquiryDetailsCommand = new Command<string>(OnViewDetails);

            _ = LoadAsync();
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public ObservableCollection<InquiryListItemVm> InquiryList { get; } = new();

        public ICommand FilterCommand { get; }
        public ICommand ViewInquiryDetailsCommand { get; }

        public async Task LoadAsync()
        {
            InquiryList.Clear();

            var items = await _inquiryService.GetInquiriesAsync(SearchQuery);
            foreach (var i in items)
            {
                InquiryList.Add(new InquiryListItemVm
                {
                    InquiryId = i.InquiryId,
                    PatientIc = i.PatientIc,
                    PatientName = i.PatientName,
                    SymptomSnippet = BuildSnippet(i.FullSymptomDescription),
                    Status = i.Status
                });
            }
        }

        private void OnViewDetails(string inquiryId)
        {
            if (string.IsNullOrWhiteSpace(inquiryId))
                return;

            Shell.Current.GoToAsync($"InquiryDetails?inquiryId={Uri.EscapeDataString(inquiryId)}");
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
    }

    public sealed class InquiryListItemVm
    {
        public string InquiryId { get; init; } = string.Empty;
        public string PatientIc { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public string SymptomSnippet { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
    }
}
