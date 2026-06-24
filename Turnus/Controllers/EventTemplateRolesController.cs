
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class EventTemplateRolesController : Controller
{
    private readonly TurnusContext _context;

    public EventTemplateRolesController(TurnusContext context)
    {
        _context = context;
    }

    // GET: EVENTTEMPLATEROLES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.EventTemplateRole.ToListAsync());
    }

    // GET: EVENTTEMPLATEROLES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventtemplaterole = await _context.EventTemplateRole
            .FirstOrDefaultAsync(m => m.Id == id);
        if (eventtemplaterole == null)
        {
            return NotFound();
        }

        return View(eventtemplaterole);
    }

    // GET: EVENTTEMPLATEROLES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: EVENTTEMPLATEROLES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,EventTemplateId,EventTemplate,RoleId,Role,RequiredCount")] EventTemplateRole eventtemplaterole)
    {
        if (ModelState.IsValid)
        {
            _context.Add(eventtemplaterole);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(eventtemplaterole);
    }

    // GET: EVENTTEMPLATEROLES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventtemplaterole = await _context.EventTemplateRole.FindAsync(id);
        if (eventtemplaterole == null)
        {
            return NotFound();
        }
        return View(eventtemplaterole);
    }

    // POST: EVENTTEMPLATEROLES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,EventTemplateId,EventTemplate,RoleId,Role,RequiredCount")] EventTemplateRole eventtemplaterole)
    {
        if (id != eventtemplaterole.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(eventtemplaterole);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventTemplateRoleExists(eventtemplaterole.Id))
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
        return View(eventtemplaterole);
    }

    // GET: EVENTTEMPLATEROLES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventtemplaterole = await _context.EventTemplateRole
            .FirstOrDefaultAsync(m => m.Id == id);
        if (eventtemplaterole == null)
        {
            return NotFound();
        }

        return View(eventtemplaterole);
    }

    // POST: EVENTTEMPLATEROLES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var eventtemplaterole = await _context.EventTemplateRole.FindAsync(id);
        if (eventtemplaterole != null)
        {
            _context.EventTemplateRole.Remove(eventtemplaterole);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EventTemplateRoleExists(int? id)
    {
        return _context.EventTemplateRole.Any(e => e.Id == id);
    }
}
