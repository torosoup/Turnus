using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public ICollection<ShiftDefinition> ShiftDefinitions { get; set; } = new List<ShiftDefinition>();
        public ICollection<VenueStaffingRequirement> VenueStaffingRequirements { get; set; } = new List<VenueStaffingRequirement>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}