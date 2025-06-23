using System;
using System.ComponentModel.DataAnnotations;

namespace GFVHaveserviceSQL.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Phone]
        public string? Telephone { get; set; }

        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastVisited { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextVisit { get; set; }
    }
}
