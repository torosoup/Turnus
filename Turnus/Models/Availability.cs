using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class Availability
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [ForeignKey("EmployeeId")]
        public ApplicationUser? Employee { get; set; }

        [Required]
        public int ScheduledShiftId { get; set; }

        [ForeignKey("ScheduledShiftId")]
        public ScheduledShift? ScheduledShift { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        // Workspace tenancy
        public int? WorkspaceId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }
    }
}