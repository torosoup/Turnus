using Turnus.Models;

namespace Turnus.Models
{
    public class ScheduleReviewDayWideViewModel
    {
        public List<ScheduledShift> Shifts { get; set; } = [];

        public List<VenueStaffingRequirement> Requirements { get; set; } = [];

        public Venue Venue { get; set; } = null!;

        public DateTime Date { get; set; }
    }
}