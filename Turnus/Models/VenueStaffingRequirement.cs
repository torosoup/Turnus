using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class VenueStaffingRequirement
    {
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [Required]
        [Range(1, 20)]
        public int RequiredCount { get; set; }

        [Display(Name = "Needed per shift (not per day)")]
        public bool IsShiftScoped { get; set; } = false; // change this later so needed per shift is default, and needed per day is optional

        // Workspace tenancy
        public int? WorkspaceId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }
    }
}