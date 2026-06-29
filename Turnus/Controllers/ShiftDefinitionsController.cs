
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class ShiftDefinitionsController : Controller
{
    private readonly TurnusContext _context;

    public ShiftDefinitionsController(TurnusContext context)
    {
        _context = context;
    }

    // GET: SHIFTDEFINITIONS
    public async Task<IActionResult> Index()
    {
        return View(await _context.ShiftDefinition
            .Include(s => s.Venue)
            .ToListAsync());
    }

    // GET: SHIFTDEFINITIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shiftdefinition = await _context.ShiftDefinition
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shiftdefinition == null)
        {
            return NotFound();
        }

        return View(shiftdefinition);
    }

    // GET: SHIFTDEFINITIONS/Create
    public IActionResult Create()
    {
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name");
        return View();
    }

    // POST: SHIFTDEFINITIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,VenueId,Name,StartTime,EndTime")] ShiftDefinition shiftdefinition)
    {
        if (ModelState.IsValid)
        {
            _context.Add(shiftdefinition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(shiftdefinition);
    }

    // GET: SHIFTDEFINITIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shiftdefinition = await _context.ShiftDefinition.FindAsync(id);
        if (shiftdefinition == null)
        {
            return NotFound();
        }
        ViewData["VenueId"] = new SelectList(_context.Venue, "Id", "Name", shiftdefinition.VenueId);
        return View(shiftdefinition);
    }

    // POST: SHIFTDEFINITIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,VenueId,Name,StartTime,EndTime")] ShiftDefinition shiftdefinition)
    {
        if (id != shiftdefinition.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(shiftdefinition);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftDefinitionExists(shiftdefinition.Id))
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
        return View(shiftdefinition);
    }

    // GET: SHIFTDEFINITIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shiftdefinition = await _context.ShiftDefinition
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shiftdefinition == null)
        {
            return NotFound();
        }

        return View(shiftdefinition);
    }

    // POST: SHIFTDEFINITIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var shiftdefinition = await _context.ShiftDefinition.FindAsync(id);
        if (shiftdefinition != null)
        {
            _context.ShiftDefinition.Remove(shiftdefinition);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ShiftDefinitionExists(int? id)
    {
        return _context.ShiftDefinition.Any(e => e.Id == id);
    }
}
