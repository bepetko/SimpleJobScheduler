using System.ComponentModel.DataAnnotations;

namespace SimpleJobScheduler.Models
{
    public class AddJobViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000)]
        public int IntervalInMinutes { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
