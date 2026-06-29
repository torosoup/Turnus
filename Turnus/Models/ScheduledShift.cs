using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class ScheduledShift
    {
        public int Id { get; set; }

        [Required]
        public int ScheduledDayId { get; set; }

        [ForeignKey("ScheduledDayId")]
        public ScheduledDay? ScheduledDay { get; set; }

        [Required]
        public int ShiftDefinitionId { get; set; }

        [ForeignKey("ShiftDefinitionId")]
        public ShiftDefinition? ShiftDefinition { get; set; }
    }
}