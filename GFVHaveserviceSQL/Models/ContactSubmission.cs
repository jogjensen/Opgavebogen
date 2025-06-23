using System;
using System.ComponentModel.DataAnnotations;

namespace GFVHaveserviceSQL.Models
{
    public class ContactSubmission
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? PlaceOfWork { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

        public bool IsSeen { get; set; } = false;
    }
}
