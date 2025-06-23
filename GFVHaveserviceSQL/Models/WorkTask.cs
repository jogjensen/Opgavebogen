using System;
using System.ComponentModel.DataAnnotations;

namespace GFVHaveserviceSQL.Models
{
    public class WorkTask
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? TaskDescription { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ScheduledDate { get; set; }

        // Recurrence settings
        public bool IsRecurring { get; set; } = false;
        public int? RepeatEveryDays { get; set; }
        public string? RecurrenceDays { get; set; }
        [DataType(DataType.Date)]
        public DateTime? RecurrenceEndDate { get; set; }

        public ICollection<ApplicationUser> AssignedWorkers { get; set; } = new List<ApplicationUser>();

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Total time spent on the task in minutes
        public int TakenTime { get; set; }

        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.New;

        public DateTime? CompletedOn { get; set; }
    }
}
