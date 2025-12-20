namespace ClinicMiniProject.Dtos
{
    public class ConsultationQueueDto
    {
        public string ConsultationId { get; set; }
        public string PatientName { get; set; }
        public string PatientIC { get; set; }
        public string ServiceType { get; set; }
        public string AppointedTime { get; set; }
        public string Date { get; set; }

    }
}