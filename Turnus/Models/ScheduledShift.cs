using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class ScheduledShift
    {
        public int Id { get; set; }

        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [Required]
        public int ShiftDefinitionId { get; set; }

        [ForeignKey("ShiftDefinitionId")]
        public ShiftDefinition? ShiftDefinition { get; set; }

        // Workspace tenancy
        public int? WorkspaceId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
        public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    }
}