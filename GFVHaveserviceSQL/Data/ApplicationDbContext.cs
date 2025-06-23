using GFVHaveserviceSQL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GFVHaveserviceSQL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Tasks)
                .WithMany(t => t.AssignedWorkers)
                .UsingEntity(j => j.ToTable("TaskWorkers"));
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
        public DbSet<WorkLog> WorkLogs => Set<WorkLog>();
        public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    }
}
