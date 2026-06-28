
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class ScheduledEventsController : Controller
{
    private readonly TurnusContext _context;

    public ScheduledEventsController(TurnusContext context)
    {
        _context = context;
    }

    // GET: SCHEDULEDEVENTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ScheduledEvent
            .Include(s => s.EventTemplate)
            .ToListAsync());
    }

    // GET: SCHEDULEDEVENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var scheduledevent = await _context.ScheduledEvent
            .FirstOrDefaultAsync(m => m.Id == id);
        if (scheduledevent == null)
        {
            return NotFound();
        }

        return View(scheduledevent);
    }

    // GET: SCHEDULEDEVENTS/Create
    public IActionResult Create()
    {
        ViewData["EventTemplateId"] = new SelectList(_context.EventTemplate, "Id", "Name");
        return View();
    }

    // POST: SCHEDULEDEVENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,EventTemplateId,EventTemplate,Date,ShiftStart,ShiftEnd")] ScheduledEvent scheduledevent)
    {
        if (ModelState.IsValid)
        {
            _context.Add(scheduledevent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(scheduledevent);
    }

    // GET: SCHEDULEDEVENTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var scheduledevent = await _context.ScheduledEvent.FindAsync(id);
        if (scheduledevent == null)
        {
            return NotFound();
        }

        ViewData["EventTemplateId"] = new SelectList(_context.EventTemplate, "Id", "Name", scheduledevent.EventTemplateId);
        return View(scheduledevent);
    }

    // POST: SCHEDULEDEVENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,EventTemplateId,EventTemplate,Date,ShiftStart,ShiftEnd")] ScheduledEvent scheduledevent)
    {
        if (id != scheduledevent.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(scheduledevent);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduledEventExists(scheduledevent.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(scheduledevent);
    }

    // GET: SCHEDULEDEVENTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var scheduledevent = await _context.ScheduledEvent
            .FirstOrDefaultAsync(m => m.Id == id);
        if (scheduledevent == null)
        {
            return NotFound();
        }

        return View(scheduledevent);
    }

    // POST: SCHEDULEDEVENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var scheduledevent = await _context.ScheduledEvent.FindAsync(id);
        if (scheduledevent != null)
        {
            _context.ScheduledEvent.Remove(scheduledevent);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ScheduledEventExists(int? id)
    {
        return _context.ScheduledEvent.Any(e => e.Id == id);
    }

    [Authorize]
    public async Task<IActionResult> Review(int id)
    {
        var scheduledEvent = await _context.ScheduledEvent
            .Include(s => s.EventTemplate)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (scheduledEvent == null)
        {
            return NotFound();
        }

        var requiredRoles = await _context.EventTemplateRole
            .Where(etr => etr.EventTemplateId == scheduledEvent.EventTemplateId)
            .Include(etr => etr.Role)
            .ToListAsync();

        var availableEmployees = await _context.Availability
            .Where(a => a.ScheduledEventId == id && a.IsAvailable)
            .Include(a => a.Employee)
            .ToListAsync();

        ViewBag.RequiredRoles = requiredRoles;
        ViewBag.AvailableEmployees = availableEmployees;

        return View(scheduledEvent);
    }
}


