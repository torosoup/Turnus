using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class VenueStaffingRequirement
    {
        public int Id { get; set; }

        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [Required]
        [Range(1, 20)]
        public int RequiredCount { get; set; }

        [Required]
        [Display(Name = "Needed per shift (not per day)")]
        public bool IsShiftScoped { get; set; }
    }
}