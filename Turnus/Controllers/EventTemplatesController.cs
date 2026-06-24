
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class EventTemplatesController : Controller
{
    private readonly TurnusContext _context;

    public EventTemplatesController(TurnusContext context)
    {
        _context = context;
    }

    // GET: EVENTTEMPLATES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.EventTemplate.ToListAsync());
    }

    // GET: EVENTTEMPLATES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventtemplate = await _context.EventTemplate
            .FirstOrDefaultAsync(m => m.Id == id);
        if (eventtemplate == null)
        {
            return NotFound();
        }

        return View(eventtemplate);
    }

    // GET: EVENTTEMPLATES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: EVENTTEMPLATES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] EventTemplate eventtemplate)
    {
        if (ModelState.IsValid)
        {
            _context.Add(eventtemplate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(eventtemplate);
    }

    // GET: EVENTTEMPLATES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventtemplate = await _context.EventTemplate.FindAsync(id);
        if (eventtemplate == null)
        {
            return NotFound();
        }
        return View(eventtemplate);
    }

    // POST: EVENTTEMPLATES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name")] EventTemplate eventtemplate)
    {
        if (id != eventtemplate.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(eventtemplate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventTemplateExists(eventtemplate.Id))
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
        return View(eventtemplate);
    }

    // GET: EVENTTEMPLATES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventtemplate = await _context.EventTemplate
            .FirstOrDefaultAsync(m => m.Id == id);
        if (eventtemplate == null)
        {
            return NotFound();
        }

        return View(eventtemplate);
    }

    // POST: EVENTTEMPLATES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var eventtemplate = await _context.EventTemplate.FindAsync(id);
        if (eventtemplate != null)
        {
            _context.EventTemplate.Remove(eventtemplate);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EventTemplateExists(int? id)
    {
        return _context.EventTemplate.Any(e => e.Id == id);
    }
}
