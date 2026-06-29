using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize]
    public class ScheduleReviewController : Controller
    {
        private readonly TurnusContext _context;

        public ScheduleReviewController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Review(int id)
        {
            var scheduledDay = await _context.ScheduledDay
                .Include(d => d.Venue)
                .Include(d => d.ScheduledShifts)
                    .ThenInclude(s => s.ShiftDefinition)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (scheduledDay == null)
            {
                return NotFound();
            }

            var requirements = await _context.VenueStaffingRequirement
                .Where(r => r.VenueId == scheduledDay.VenueId)
                .Include(r => r.Role)
                .ToListAsync();

            var shiftIds = scheduledDay.ScheduledShifts.Select(s => s.Id).ToList();

            var availability = await _context.Availability
                .Where(a => shiftIds.Contains(a.ScheduledShiftId) && a.IsAvailable)
                .Include(a => a.Employee)
                .Include(a => a.ScheduledShift)
                .ToListAsync();

            ViewBag.Requirements = requirements;
            ViewBag.Availability = availability;

            return View(scheduledDay);
        }
    }
}