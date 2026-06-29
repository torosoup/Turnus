
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class VenuesController : Controller
{
    private readonly TurnusContext _context;

    public VenuesController(TurnusContext context)
    {
        _context = context;
    }

    // GET: VENUES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Venue.ToListAsync());
    }

    // GET: VENUES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venue
            .FirstOrDefaultAsync(m => m.Id == id);
        if (venue == null)
        {
            return NotFound();
        }

        return View(venue);
    }

    // GET: VENUES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: VENUES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,VenueStaffingRequirements,ShiftDefinitions,ScheduledDays")] Venue venue)
    {
        if (ModelState.IsValid)
        {
            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(venue);
    }

    // GET: VENUES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venue.FindAsync(id);
        if (venue == null)
        {
            return NotFound();
        }
        return View(venue);
    }

    // POST: VENUES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,VenueStaffingRequirements,ShiftDefinitions,ScheduledDays")] Venue venue)
    {
        if (id != venue.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(venue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.Id))
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
        return View(venue);
    }

    // GET: VENUES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venue
            .FirstOrDefaultAsync(m => m.Id == id);
        if (venue == null)
        {
            return NotFound();
        }

        return View(venue);
    }

    // POST: VENUES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var venue = await _context.Venue.FindAsync(id);
        if (venue != null)
        {
            _context.Venue.Remove(venue);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool VenueExists(int? id)
    {
        return _context.Venue.Any(e => e.Id == id);
    }
}
