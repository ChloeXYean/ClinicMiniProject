using ClinicMiniProject.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ClinicMiniProject.ViewModels
{
    public class WalkInPatientQueueViewModel : BindableObject
    {
        private readonly AppDbContext _context;
        public ObservableCollection<PatientQueueDto> QueueList { get; set; } = new();
        public string CurrentDate { get; } = DateTime.Now.ToString("dd MMMM yyyy");
        public ICommand BackCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public WalkInPatientQueueViewModel(AppDbContext context)
        {
            _context = context;

            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ViewDetailsCommand = new Command<PatientQueueDto>(OnViewDetails);
        }

        public async Task LoadQueue()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    QueueList.Clear();

                    var today = DateTime.Today;

                    var appointments = await _context.Appointments
                        .Include(a => a.Patient)
                        .Where(a => a.appointedAt.HasValue && a.appointedAt.Value.Date == today)
                        .OrderBy(a => a.appointedAt)
                        .ToListAsync();
                    foreach (var appt in appointments)
                    {
                        QueueList.Add(new PatientQueueDto
                        {
                            PatientName = appt.Patient?.patient_name ?? "Unknown",
                            QueueId = appt.appointment_ID,
                            ICNumber = appt.patient_IC,
                            RegisteredTime = $"{appt.bookedAt:hh:mm tt} (Slot: {appt.appointedAt:hh:mm tt})",
                            PhoneNumber = appt.Patient?.patient_contact ?? "N/A",
                            ServiceType = appt.service_type,
                            Status = appt.status
                        });
                    }

                    if (QueueList.Count == 0)
                    {
                        Console.WriteLine("Debug: No appointments found for today.");
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to load queue: {ex.Message}", "OK");
                }
            });
        }

        private async void OnViewDetails(PatientQueueDto patient)
        {
            if (patient == null) return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedPatient", patient }
            };
            // Ensure this route is registered in AppShell!
            await Shell.Current.GoToAsync("PatientDetails", navigationParameter);
        }
    }
}