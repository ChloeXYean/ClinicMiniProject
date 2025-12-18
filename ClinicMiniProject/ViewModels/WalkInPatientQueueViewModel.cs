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
                        .Where(a => a.appointedAt.Value.Date == today 
                                    && a.status == "Pending")         
                        .OrderBy(a => a.appointedAt)                   
                        .ToListAsync();

                    foreach (var appt in appointments)
                    {
                        QueueList.Add(new PatientQueueDto
                        {
                            PatientName = appt.Patient?.patient_name ?? "Unknown",
                            QueueId = appt.appointment_ID,
                            RegisteredTime = appt.appointedAt?.ToString("hh:mm tt") ?? "--",
                            ICNumber = appt.patient_IC
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