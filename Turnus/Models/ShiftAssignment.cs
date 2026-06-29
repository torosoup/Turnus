using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public class ShiftAssignment
    {
        public int Id { get; set; }

        [Required]
        public int ScheduledShiftId { get; set; }

        [ForeignKey("ScheduledShiftId")]
        public ScheduledShift? ScheduledShift { get; set; }

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