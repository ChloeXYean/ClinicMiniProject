namespace ClinicMiniProject.Dtos
{
    public class PatientQueueDto
    {
        public string PatientName { get; set; }
        public string QueueId { get; set; }
        public string ICNumber { get; set; }

        public string RegisteredTime { get; set; }
        public string PhoneNumber { get; set; }
    }
}