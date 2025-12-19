using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicMiniProject.Services.Interfaces;

namespace ClinicMiniProject.ViewModels
{
    public class ConsultationSessionViewModel : INotifyPropertyChanged
    {
        private readonly IConsultationService _consultationService;

        private ConsultationDetailsDto? _details;
        private string _remark = string.Empty;

        public ConsultationDetailsDto? Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        public ICommand EndConsultationCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConsultationSessionViewModel(IConsultationService consultationService)
        {
            _consultationService = consultationService;
            EndConsultationCommand = new Command(async () => await EndAsync());
        }

        public async Task LoadByAppointmentIdAsync(string appointmentId)
        {
            Details = await _consultationService.GetConsultationDetailsByAppointmentIdAsync(appointmentId);
        }

        public async Task EndAsync()
        {
            if (Details == null)
                return;

            await _consultationService.EndConsultationAsync(Details.AppointmentId, Remark);
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
