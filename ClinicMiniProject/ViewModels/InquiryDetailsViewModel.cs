using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public sealed class InquiryDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IInquiryService _inquiryService;

        private string _inquiryId = string.Empty;
        private InquiryDto? _details;
        private string _doctorResponseText = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public InquiryDetailsViewModel(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;

            SendResponseCommand = new Command(async () => await SendAsync());
            ViewFullProfileCommand = new Command(OnViewFullProfile);
        }

        public string InquiryId
        {
            get => _inquiryId;
            set
            {
                if (SetProperty(ref _inquiryId, value))
                {
                    _ = LoadAsync();
                }
            }
        }

        public InquiryDto? Details
        {
            get => _details;
            private set
            {
                if (SetProperty(ref _details, value))
                {
                    OnPropertyChanged(nameof(PatientName));
                    OnPropertyChanged(nameof(PatientAgeGender));
                    OnPropertyChanged(nameof(PatientIc));
                    OnPropertyChanged(nameof(FullSymptomDescription));
                    OnPropertyChanged(nameof(ImageSource1));
                    OnPropertyChanged(nameof(ImageSource2));
                    OnPropertyChanged(nameof(ImageSource3));
                }
            }
        }

        public string PatientName => Details?.PatientName ?? string.Empty;
        public string PatientAgeGender => Details == null ? string.Empty : $"{Details.PatientAge} / {Details.PatientGender}";
        public string PatientIc => Details?.PatientIc ?? string.Empty;
        public string FullSymptomDescription => Details?.FullSymptomDescription ?? string.Empty;

        public ImageSource? ImageSource1 => ResolveImage(Details?.Image1);
        public ImageSource? ImageSource2 => ResolveImage(Details?.Image2);
        public ImageSource? ImageSource3 => ResolveImage(Details?.Image3);

        public string DoctorResponseText
        {
            get => _doctorResponseText;
            set => SetProperty(ref _doctorResponseText, value);
        }

        public ICommand SendResponseCommand { get; }
        public ICommand ViewFullProfileCommand { get; }

        private async Task LoadAsync()
        {
            if (string.IsNullOrWhiteSpace(InquiryId))
                return;

            Details = await _inquiryService.GetInquiryByIdAsync(InquiryId);
            DoctorResponseText = Details?.DoctorResponse ?? string.Empty;
        }

        private async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(InquiryId))
                return;

            await _inquiryService.SendResponseAsync(InquiryId, DoctorResponseText);
            await Shell.Current.GoToAsync("..");
        }

        private void OnViewFullProfile()
        {
            var ic = Details?.PatientIc;
            if (string.IsNullOrWhiteSpace(ic))
                return;

            Shell.Current.GoToAsync($"PatientDetails?patientIc={Uri.EscapeDataString(ic)}");
        }

        private static ImageSource? ResolveImage(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return ImageSource.FromFile(path);
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
}
