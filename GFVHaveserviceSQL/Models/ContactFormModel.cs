using System.ComponentModel.DataAnnotations;

namespace GFVHaveserviceSQL.Models
{
    public class ContactFormModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "PlaceOfWork")]
        public string? PlaceOfWork { get; set; }
    }
}
