namespace SimpleJobScheduler.Models.Domain
{
    public class JobsHistory
    {
        public Guid Id { get; set; }
        public DateTime ExecutionDate { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Response { get; set; } = null!;
        public virtual Job Job { get; set; } = null!;
        public Guid JobId { get; set; }
    }
}
