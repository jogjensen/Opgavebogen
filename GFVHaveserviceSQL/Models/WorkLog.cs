using System;
using System.ComponentModel.DataAnnotations;

namespace GFVHaveserviceSQL.Models
{
    public class WorkLog
    {
        public int Id { get; set; }

        [Required]
        public int WorkTaskId { get; set; }
        public WorkTask? WorkTask { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? EquipmentUsed { get; set; }

        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
    }
}
