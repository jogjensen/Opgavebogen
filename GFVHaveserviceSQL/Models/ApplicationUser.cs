using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace GFVHaveserviceSQL.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }

        public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    }
}
