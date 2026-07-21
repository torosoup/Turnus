namespace Turnus.Models
{
    public class AvailableEmployeesViewModel
    {
        public ScheduledShift Shift { get; set; } = null!;
        public VenueStaffingRequirement Requirement { get; set; } = null!;
        public List<Availability> Available { get; set; } = [];
        public List<ShiftAssignment> Assigned { get; set; } = [];
        public Venue Venue { get; set; } = null!;
        public DateTime Date { get; set; }
    }
}