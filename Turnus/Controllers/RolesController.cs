
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
    public async Task<IActionResult> Create()
    {
        ViewBag.Venues = await _context.Venue.ToListAsync();
        ViewBag.Departments = await _context.Department.ToListAsync();

        return PartialView(
            "~/Views/Admin/Partials/Configuration/Role/_CreateRole.cshtml",
            new Role());
    }

    // POST: ROLES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Role role)
    {
        if (ModelState.IsValid)
        {
            _context.Add(role);
            await _context.SaveChangesAsync();

            // Preserve dashboard context: role -> department -> venue
            var dept = await _context.Department.FindAsync(role.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = role.DepartmentId });
        }

        ViewBag.Venues = await _context.Venue.ToListAsync();
        ViewBag.Departments = await _context.Department.ToListAsync();

        return PartialView(
            "~/Views/Admin/Partials/Configuration/Role/_CreateRole.cshtml",
            role);
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

        ViewBag.Venues = await _context.Venue.ToListAsync();
        ViewBag.Departments = await _context.Department.ToListAsync();

        return PartialView(
            "~/Views/Admin/Partials/Configuration/Role/_EditRole.cshtml",
            role);
    }

    // POST: ROLES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, Role role)
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

                throw;
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        ViewBag.Venues = await _context.Venue.ToListAsync();
        ViewBag.Departments = await _context.Department.ToListAsync();

        return PartialView(
            "~/Views/Admin/Partials/Configuration/Role/_EditRole.cshtml",
            role);
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

        return PartialView("~/Views/Admin/Partials/Configuration/Role/_DeleteRole.cshtml", role);
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

        // Redirect back to the dashboard with context for the deleted role
        var deptForDeleted = await _context.Department.FindAsync(role?.DepartmentId);
        return RedirectToAction("Dashboard", "Admin", new { venueId = deptForDeleted?.VenueId, departmentId = role?.DepartmentId });
    }

    private bool RoleExists(int? id)
    {
        return _context.Role.Any(e => e.Id == id);
    }
}
