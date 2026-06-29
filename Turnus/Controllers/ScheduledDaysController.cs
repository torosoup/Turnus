using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class ScheduledDaysController : Controller
{
    private readonly TurnusContext _context;

    public ScheduledDaysController(TurnusContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.ScheduledDay
            .Include(d => d.Venue)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var scheduledDay = await _context.ScheduledDay
            .Include(d => d.Venue)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (scheduledDay == null) return NotFound();

        return View(scheduledDay);
    }

    public IActionResult Create()
    {
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,VenueId,Date")] ScheduledDay scheduledDay)
    {
        if (ModelState.IsValid)
        {
            _context.Add(scheduledDay);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", scheduledDay.VenueId);
        return View(scheduledDay);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var scheduledDay = await _context.ScheduledDay.FindAsync(id);
        if (scheduledDay == null) return NotFound();

        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", scheduledDay.VenueId);
        return View(scheduledDay);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,Date")] ScheduledDay scheduledDay)
    {
        if (id != scheduledDay.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(scheduledDay);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduledDayExists(scheduledDay.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", scheduledDay.VenueId);
        return View(scheduledDay);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var scheduledDay = await _context.ScheduledDay
            .Include(d => d.Venue)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (scheduledDay == null) return NotFound();

        return View(scheduledDay);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var scheduledDay = await _context.ScheduledDay.FindAsync(id);
        if (scheduledDay != null)
        {
            _context.ScheduledDay.Remove(scheduledDay);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ScheduledDayExists(int id)
    {
        return _context.ScheduledDay.Any(e => e.Id == id);
    }
}