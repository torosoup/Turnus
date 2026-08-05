using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class StaffingRequirementsController : Controller
    {
        private readonly TurnusContext _context;

        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public StaffingRequirementsController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
        }

        public async Task<IActionResult> Create(int departmentId)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            ViewBag.Roles = await _context.Role
                .Where(r => r.DepartmentId == departmentId && r.WorkspaceId == wsId.Value)
                .ToListAsync();

            return PartialView("~/Views/Admin/Partials/Configuration/StaffingRequirement/_CreateStaffingRequirement.cshtml",
                new VenueStaffingRequirement { DepartmentId = departmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueStaffingRequirement model)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var role = await _context.Role.FindAsync(model.RoleId);

            if (role == null || role.DepartmentId != model.DepartmentId || role.WorkspaceId != wsId.Value)
            {
                ModelState.AddModelError("RoleId", "Selected role does not belong to this department or workspace.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Role
                    .Where(r => r.DepartmentId == model.DepartmentId && r.WorkspaceId == wsId.Value)
                    .ToListAsync();

                return PartialView(
                    "~/Views/Admin/Partials/Configuration/StaffingRequirement/_CreateStaffingRequirement.cshtml",
                    model);
            }

            model.WorkspaceId = wsId.Value;
            _context.VenueStaffingRequirement.Add(model);
            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(model.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = model.DepartmentId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var req = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == wsId.Value);

            if (req == null) return NotFound();

            ViewBag.Roles = await _context.Role.Where(r => r.WorkspaceId == wsId.Value).ToListAsync();

            return PartialView("~/Views/Admin/Partials/Configuration/StaffingRequirement/_EditStaffingRequirement.cshtml", req);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VenueStaffingRequirement model)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var role = await _context.Role.FindAsync(model.RoleId);

            if (role == null || role.DepartmentId != model.DepartmentId || role.WorkspaceId != wsId.Value)
            {
                ModelState.AddModelError("RoleId", "Selected role does not belong to this department or workspace.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Role
                    .Where(r => r.DepartmentId == model.DepartmentId && r.WorkspaceId == wsId.Value)
                    .ToListAsync();

                return PartialView(
                    "~/Views/Admin/Partials/Configuration/StaffingRequirement/_EditStaffingRequirement.cshtml",
                    model);
            }

            model.WorkspaceId = wsId.Value;
            _context.Update(model);
            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(model.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = model.DepartmentId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var req = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == wsId.Value);

            if (req == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/Configuration/StaffingRequirement/_DeleteStaffingRequirement.cshtml", req);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(VenueStaffingRequirement model)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var req = await _context.VenueStaffingRequirement.FindAsync(model.Id);
            if (req == null || req.WorkspaceId != wsId.Value) return NotFound();

            _context.VenueStaffingRequirement.Remove(req);
            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(req.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = req.DepartmentId });
        }
    }
}
