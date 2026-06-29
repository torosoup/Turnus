using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class DayAssignment
    {
        public int Id { get; set; }

        [Required]
        public int ScheduledDayId { get; set; }

        [ForeignKey("ScheduledDayId")]
        public ScheduledDay? ScheduledDay { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [ForeignKey("EmployeeId")]
        public ApplicationUser? Employee { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }
}