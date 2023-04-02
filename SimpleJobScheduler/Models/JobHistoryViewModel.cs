namespace SimpleJobScheduler.Models
{
    public class JobHistoryViewModel
    {
        public string JobName { get; set; }
        public Guid JobId { get; set; }
        public List<JobExecutionHistoryViewModel> JobExecutionHistoryList { get; set; }

    }
}
