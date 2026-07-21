namespace Turnus.Models
{
    public class ScheduleReviewViewModel
    {
        public ScheduledShift Shift { get; set; } = null!;

        public List<VenueStaffingRequirement> Requirements { get; set; } = [];

        public List<Availability> Availability { get; set; } = [];

        public Venue Venue { get; set; } = null!;

        public DateTime Date { get; set; }
    }
}
