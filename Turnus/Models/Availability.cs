using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class Availability
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty; // Primary key to EmployeeId is a string.

        [ForeignKey("EmployeeId")]
        public ApplicationUser? Employee { get; set; }

        [Required]
        public int ScheduledEventId { get; set; }

        [ForeignKey("ScheduledEventId")]
        public ScheduledEvent? ScheduledEvent { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}