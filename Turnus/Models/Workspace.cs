using System.ComponentModel.DataAnnotations;

namespace Turnus.Models
{
    public class Workspace
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Default";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }

        // Navigation
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public ICollection<Venue> Venues { get; set; } = new List<Venue>();
    }
}
