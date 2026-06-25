using Microsoft.AspNetCore.Identity;

namespace Turnus.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}