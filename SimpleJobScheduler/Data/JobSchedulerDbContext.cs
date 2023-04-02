using Microsoft.EntityFrameworkCore;
using SimpleJobScheduler.Models.Domain;

namespace SimpleJobScheduler.Data;
public class JobSchedulerDbContext : DbContext
{
    public JobSchedulerDbContext(DbContextOptions options) : base(options) { }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobsHistory> JobsHistory { get; set; }
}

