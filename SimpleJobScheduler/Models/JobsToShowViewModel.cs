namespace SimpleJobScheduler.Models
{
    public class JobsToShowViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        public string? Result { get; set; }
        public DateTime? NextExecutionTime { get; set; }

    }
}
