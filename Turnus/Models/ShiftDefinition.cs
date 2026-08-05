using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class ShiftDefinition
    {
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        // Workspace tenancy
        public int? WorkspaceId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }
}