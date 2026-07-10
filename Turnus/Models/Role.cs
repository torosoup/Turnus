using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Scope { get; set; } = "General"; // General, Venue, Department

        public int? VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
    }
}