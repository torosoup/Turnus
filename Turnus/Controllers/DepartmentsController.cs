using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class DepartmentsController : Controller
    {
        private readonly TurnusContext _context;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public DepartmentsController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
        }

        public async Task<IActionResult> Index()
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            return View(await _context.Department
                .Include(d => d.Venue)
                .Where(d => d.WorkspaceId == wsId.Value)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Department
                .Include(d => d.Venue)
                .Include(d => d.ShiftDefinitions)
                .Include(d => d.VenueStaffingRequirements)
                    .ThenInclude(r => r.Role)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            return View(department);
        }

        public async Task<IActionResult> Create(int? venueId)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            ViewData["VenueId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync(),
                "Id", "Name", venueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_CreateDepartment.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VenueId,Name")] Department department)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            // Ensure the selected venue belongs to the workspace
            var venue = await _context.Venue.FindAsync(department.VenueId);
            if (venue == null || venue.WorkspaceId != wsId.Value)
            {
                ModelState.AddModelError("VenueId", "Selected venue is invalid.");
            }

            if (ModelState.IsValid)
            {
                department.WorkspaceId = wsId.Value;
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Admin", new { venueId = department.VenueId, departmentId = department.Id });
            }

            ViewData["VenueId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync(),
                "Id", "Name", department.VenueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_CreateDepartment.cshtml", department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var department = await _context.Department.FindAsync(id);
            if (department == null || department.WorkspaceId != wsId.Value) return NotFound();

            ViewData["VenueId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync(),
                "Id", "Name", department.VenueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_EditDepartment.cshtml", department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,Name")] Department department)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            if (id != department.Id) return NotFound();

            // Ensure existing department belongs to workspace and venue belongs to workspace
            var existing = await _context.Department.FindAsync(id);
            if (existing == null || existing.WorkspaceId != wsId.Value) return NotFound();

            var venue = await _context.Venue.FindAsync(department.VenueId);
            if (venue == null || venue.WorkspaceId != wsId.Value)
            {
                ModelState.AddModelError("VenueId", "Selected venue is invalid.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    department.WorkspaceId = wsId.Value;
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Department.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Dashboard", "Admin");
            }

            ViewData["VenueId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Venue.Where(v => v.WorkspaceId == wsId.Value).ToListAsync(),
                "Id", "Name", department.VenueId);
            return PartialView("~/Views/Admin/Partials/Configuration/Department/_EditDepartment.cshtml", department);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var department = await _context.Department
                .Include(d => d.Venue)
                .FirstOrDefaultAsync(d => d.Id == id && d.WorkspaceId == wsId.Value);

            if (department == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/Department/_DeleteDepartment.cshtml", department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var department = await _context.Department.FindAsync(id);
            if (department == null || department.WorkspaceId != wsId.Value) return NotFound();

            _context.Department.Remove(department);
            await _context.SaveChangesAsync();
            // After deletion, preserve venue context but department no longer exists
            return RedirectToAction("Dashboard", "Admin", new { venueId = department?.VenueId });
        }
    }
}