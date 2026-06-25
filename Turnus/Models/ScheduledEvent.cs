using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class ScheduledEvent
    {
        public int Id { get; set; }

        [Required]
        public int EventTemplateId { get; set; }

        [ForeignKey("EventTemplateId")]
        public EventTemplate? EventTemplate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan ShiftStart { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan ShiftEnd { get; set; }
    }
}