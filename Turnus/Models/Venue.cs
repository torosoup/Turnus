using System.ComponentModel.DataAnnotations;

namespace Turnus.Models
{
    public class Venue
    {
        public int Id { get; set; }

        // Workspace tenancy
        public int? WorkspaceId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<ScheduledShift> ScheduledShifts { get; set; } = new List<ScheduledShift>();
    }
}