using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class ScheduledShiftsController : Controller
{
    private readonly TurnusContext _context;

    public ScheduledShiftsController(TurnusContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.ScheduledShift
            .Include(s => s.ScheduledDay)
                .ThenInclude(d => d!.Venue)
            .Include(s => s.ShiftDefinition)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var scheduledShift = await _context.ScheduledShift
            .Include(s => s.ScheduledDay)
                .ThenInclude(d => d!.Venue)
            .Include(s => s.ShiftDefinition)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (scheduledShift == null) return NotFound();

        return View(scheduledShift);
    }

    public IActionResult Create()
    {
        ViewData["ScheduledDayId"] = new SelectList(
            _context.ScheduledDay.Include(d => d.Venue)
                .Select(d => new { d.Id, Display = d.Venue!.Name + " — " + d.Date.ToShortDateString() }), // Combined label
            "Id", "Display");

        ViewData["ShiftDefinitionId"] = new SelectList(_context.ShiftDefinition, "Id", "Name");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ScheduledDayId,ShiftDefinitionId")] ScheduledShift scheduledShift)
    {
        if (ModelState.IsValid)
        {
            _context.Add(scheduledShift);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ScheduledDayId"] = new SelectList(
            _context.ScheduledDay.Include(d => d.Venue)
                .Select(d => new { d.Id, Display = d.Venue!.Name + " — " + d.Date.ToShortDateString() }),
            "Id", "Display", scheduledShift.ScheduledDayId);

        ViewData["ShiftDefinitionId"] = new SelectList(_context.ShiftDefinition, "Id", "Name", scheduledShift.ShiftDefinitionId);

        return View(scheduledShift);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var scheduledShift = await _context.ScheduledShift.FindAsync(id);
        if (scheduledShift == null) return NotFound();

        ViewData["ScheduledDayId"] = new SelectList(
            _context.ScheduledDay.Include(d => d.Venue)
                .Select(d => new { d.Id, Display = d.Venue!.Name + " — " + d.Date.ToShortDateString() }),
            "Id", "Display", scheduledShift.ScheduledDayId);

        ViewData["ShiftDefinitionId"] = new SelectList(_context.ShiftDefinition, "Id", "Name", scheduledShift.ShiftDefinitionId);

        return View(scheduledShift);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ScheduledDayId,ShiftDefinitionId")] ScheduledShift scheduledShift)
    {
        if (id != scheduledShift.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(scheduledShift);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduledShiftExists(scheduledShift.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["ScheduledDayId"] = new SelectList(
            _context.ScheduledDay.Include(d => d.Venue)
                .Select(d => new { d.Id, Display = d.Venue!.Name + " — " + d.Date.ToShortDateString() }),
            "Id", "Display", scheduledShift.ScheduledDayId);

        ViewData["ShiftDefinitionId"] = new SelectList(_context.ShiftDefinition, "Id", "Name", scheduledShift.ShiftDefinitionId);

        return View(scheduledShift);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var scheduledShift = await _context.ScheduledShift
            .Include(s => s.ScheduledDay)
                .ThenInclude(d => d!.Venue)
            .Include(s => s.ShiftDefinition)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (scheduledShift == null) return NotFound();

        return View(scheduledShift);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var scheduledShift = await _context.ScheduledShift.FindAsync(id);
        if (scheduledShift != null)
        {
            _context.ScheduledShift.Remove(scheduledShift);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ScheduledShiftExists(int id)
    {
        return _context.ScheduledShift.Any(e => e.Id == id);
    }
}