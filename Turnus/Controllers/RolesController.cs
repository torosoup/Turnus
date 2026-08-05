
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

[Authorize(Policy = "WorkspaceManager")]
public class RolesController : Controller
{
    private readonly TurnusContext _context;
    private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

    public RolesController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
    {
        _context = context;
        _workspaceProvider = workspaceProvider;
    }

    // GET: ROLES
    public async Task<IActionResult> Index()    
    {
        var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        return View(await _context.Role.Where(r => r.WorkspaceId == wsId.Value).ToListAsync());
    }

    // GET: ROLES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        var role = await _context.Role
            .FirstOrDefaultAsync(m => m.Id == id && m.WorkspaceId == wsId.Value);
        if (role == null)
        {
            return NotFound();
        }

        return View(role);
    }

    // GET: ROLES/Create
    public async Task<IActionResult> Create()
    {
        var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        ViewBag.Venues = await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync();
        ViewBag.Departments = await _context.Department.Where(d => d.WorkspaceId == wsId.Value).ToListAsync();

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
            // Verify the department exists
            var deptCheck = await _context.Department.FindAsync(role.DepartmentId);
            if (deptCheck == null)
            {
                ModelState.AddModelError("DepartmentId", "Selected department does not exist.");
                ViewBag.Venues = await _context.Venue.ToListAsync();
                ViewBag.Departments = await _context.Department.ToListAsync();
                return PartialView(
                    "~/Views/Admin/Partials/Configuration/Role/_CreateRole.cshtml",
                    role);
            }
            _context.Add(role);
            await _context.SaveChangesAsync();
            // Preserve dashboard context: role -> department -> venue
            var dept = await _context.Department.FindAsync(role.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = role.DepartmentId });
        }

        var wsId = await HttpContext.RequestServices.GetRequiredService<Turnus.Services.ICurrentWorkspaceProvider>().GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        ViewBag.Venues = await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync();
        ViewBag.Departments = await _context.Department.Where(d => d.WorkspaceId == wsId.Value).ToListAsync();

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
        var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        var role = await _context.Role.FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == wsId.Value);

        if (role == null)
        {
            return NotFound();
        }

        ViewBag.Venues = await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync();
        ViewBag.Departments = await _context.Department.Where(d => d.WorkspaceId == wsId.Value).ToListAsync();

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
                var wsId = await HttpContext.RequestServices.GetRequiredService<Turnus.Services.ICurrentWorkspaceProvider>().GetWorkspaceIdAsync();
                if (!wsId.HasValue) return Forbid();

                // Verify the department exists and belongs to the workspace before updating
                var dept = await _context.Department.FindAsync(role.DepartmentId);
                if (dept == null || dept.WorkspaceId != wsId.Value)
                {
                    ModelState.AddModelError("DepartmentId", "Selected department does not exist.");
                    ViewBag.Venues = await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync();
                    ViewBag.Departments = await _context.Department.Where(d => d.WorkspaceId == wsId.Value).ToListAsync();
                    return PartialView(
                        "~/Views/Admin/Partials/Configuration/Role/_EditRole.cshtml",
                        role);
                }

                role.WorkspaceId = wsId.Value;
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
        var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        var role = await _context.Role
            .FirstOrDefaultAsync(m => m.Id == id && m.WorkspaceId == wsId.Value);
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
        var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
        if (!wsId.HasValue) return Forbid();

        var role = await _context.Role.FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == wsId.Value);
        if (role != null)
        {
            _context.Role.Remove(role);
            await _context.SaveChangesAsync();

            // Redirect back to the dashboard with context for the deleted role
            var deptForDeleted = await _context.Department.FindAsync(role.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = deptForDeleted?.VenueId, departmentId = role.DepartmentId });
        }

        return NotFound();
    }

    private bool RoleExists(int? id)
    {
        return _context.Role.Any(e => e.Id == id);
    }
}
