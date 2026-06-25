using System.ComponentModel.DataAnnotations;

namespace Turnus.Models
{
    public class EventTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        public ICollection<ScheduledEvent> ScheduledEvents { get; set; } = new List<ScheduledEvent>(); // MVP
    }
}