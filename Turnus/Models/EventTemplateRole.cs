using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class EventTemplateRole
    {
        public int Id { get; set; }

        [Required]
        public int EventTemplateId { get; set; }

        [ForeignKey("EventTemplateId")]
        public EventTemplate? EventTemplate { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [Required]
        [Range(1, 20)]
        public int RequiredCount { get; set; }
    }
}