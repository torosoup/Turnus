using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

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

    // GET: shows upcoming scheduled events and the user's current availability status
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var scheduledEvents = await _context.ScheduledEvent
            .Include(s => s.EventTemplate)
            .ToListAsync();

        var myAvailability = await _context.Availability
            .Where(a => a.EmployeeId == userId)
            .ToListAsync();

        ViewBag.MyAvailability = myAvailability;

        return View(scheduledEvents);
    }

    // POST: sets or updates the logged-in employee's availability for a scheduled event
    [HttpPost]
    public async Task<IActionResult> SetAvailability(int scheduledEventId, bool isAvailable)
    {
        var userId = _userManager.GetUserId(User);

        var existing = await _context.Availability
            .FirstOrDefaultAsync(a => a.EmployeeId == userId && a.ScheduledEventId == scheduledEventId);

        if (existing != null)
        {
            existing.IsAvailable = isAvailable;
        }
        else
        {
            _context.Availability.Add(new Availability
            {
                EmployeeId = userId!,
                ScheduledEventId = scheduledEventId,
                IsAvailable = isAvailable
            });
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}