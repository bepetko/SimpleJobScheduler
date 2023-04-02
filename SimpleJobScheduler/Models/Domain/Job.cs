namespace SimpleJobScheduler.Models.Domain
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int IntervalInMinutes { get; set; }
        public DateTime? DateOfLastStart { get; set; }
    }
}
