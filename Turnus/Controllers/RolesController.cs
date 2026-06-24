
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class RolesController : Controller
{
    private readonly TurnusContext _context;

    public RolesController(TurnusContext context)
    {
        _context = context;
    }

    // GET: ROLES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Role.ToListAsync());
    }

    // GET: ROLES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await _context.Role
            .FirstOrDefaultAsync(m => m.Id == id);
        if (role == null)
        {
            return NotFound();
        }

        return View(role);
    }

    // GET: ROLES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ROLES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] Role role)
    {
        if (ModelState.IsValid)
        {
            _context.Add(role);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(role);
    }

    // GET: ROLES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await _context.Role.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        return View(role);
    }

    // POST: ROLES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name")] Role role)
    {
        if (id != role.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(role.Id))
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
        return View(role);
    }

    // GET: ROLES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await _context.Role
            .FirstOrDefaultAsync(m => m.Id == id);
        if (role == null)
        {
            return NotFound();
        }

        return View(role);
    }

    // POST: ROLES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var role = await _context.Role.FindAsync(id);
        if (role != null)
        {
            _context.Role.Remove(role);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RoleExists(int? id)
    {
        return _context.Role.Any(e => e.Id == id);
    }
}
