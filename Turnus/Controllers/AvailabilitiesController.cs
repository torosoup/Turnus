using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize]
    public class AvailabilitiesController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AvailabilitiesController(TurnusContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Availabilities
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var scheduledShifts = await _context.ScheduledShift
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ScheduledDay)
                    .ThenInclude(d => d!.Venue)
                .ToListAsync();

            var myAvailability = await _context.Availability
                .Where(a => a.EmployeeId == userId)
                .ToListAsync();

            ViewBag.MyAvailability = myAvailability;

            return View(scheduledShifts);
        }

        // POST: Availabilities/SetAvailability
        [HttpPost]
        public async Task<IActionResult> SetAvailability(int scheduledShiftId, bool isAvailable)
        {
            var userId = _userManager.GetUserId(User);

            var existing = await _context.Availability
                .FirstOrDefaultAsync(a => a.EmployeeId == userId && a.ScheduledShiftId == scheduledShiftId);

            if (existing != null)
            {
                existing.IsAvailable = isAvailable;
            }
            else
            {
                _context.Availability.Add(new Availability
                {
                    EmployeeId = userId!,
                    ScheduledShiftId = scheduledShiftId,
                    IsAvailable = isAvailable
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}