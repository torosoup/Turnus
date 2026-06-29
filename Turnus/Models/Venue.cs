using System.ComponentModel.DataAnnotations;

namespace Turnus.Models
{
    public class Venue
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public ICollection<VenueStaffingRequirement> VenueStaffingRequirements { get; set; } = new List<VenueStaffingRequirement>();
        public ICollection<ShiftDefinition> ShiftDefinitions { get; set; } = new List<ShiftDefinition>();
        public ICollection<ScheduledDay> ScheduledDays { get; set; } = new List<ScheduledDay>();
    }
}