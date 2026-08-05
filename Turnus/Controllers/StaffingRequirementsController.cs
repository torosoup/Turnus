using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
    public class StaffingRequirementsController : Controller
    {
        private readonly TurnusContext _context;

        public StaffingRequirementsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int departmentId)
        {
            ViewBag.Roles = await _context.Role
                .Where(r => r.DepartmentId == departmentId)
                .ToListAsync();

            return PartialView("~/Views/Admin/Partials/Configuration/StaffingRequirement/_CreateStaffingRequirement.cshtml",
                new VenueStaffingRequirement { DepartmentId = departmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueStaffingRequirement model)
        {
            var role = await _context.Role.FindAsync(model.RoleId);

            if (role == null || role.DepartmentId != model.DepartmentId)
            {
                ModelState.AddModelError("RoleId",
                    "Selected role does not belong to this department.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Role
                    .Where(r => r.DepartmentId == model.DepartmentId)
                    .ToListAsync();

                return PartialView(
                    "~/Views/Admin/Partials/Configuration/StaffingRequirement/_CreateStaffingRequirement.cshtml",
                    model);
            }

            _context.VenueStaffingRequirement.Add(model);
            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(model.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = model.DepartmentId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var req = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id);

            ViewBag.Roles = await _context.Role.ToListAsync();

            return PartialView("~/Views/Admin/Partials/Configuration/StaffingRequirement/_EditStaffingRequirement.cshtml", req);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(VenueStaffingRequirement model)
{
    var role = await _context.Role.FindAsync(model.RoleId);

    if (role == null || role.DepartmentId != model.DepartmentId)
    {
        ModelState.AddModelError("RoleId",
            "Selected role does not belong to this department.");
    }

    if (!ModelState.IsValid)
    {
        ViewBag.Roles = await _context.Role
            .Where(r => r.DepartmentId == model.DepartmentId)
            .ToListAsync();

        return PartialView(
            "~/Views/Admin/Partials/Configuration/StaffingRequirement/_EditStaffingRequirement.cshtml",
            model);
    }

    _context.Update(model);
    await _context.SaveChangesAsync();

    var dept = await _context.Department.FindAsync(model.DepartmentId);
    return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = model.DepartmentId });
}

        public async Task<IActionResult> Delete(int id)
        {
            var req = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id);

            return PartialView("~/Views/Admin/Partials/Configuration/StaffingRequirement/_DeleteStaffingRequirement.cshtml", req);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(VenueStaffingRequirement model)
        {
            var req = await _context.VenueStaffingRequirement.FindAsync(model.Id);
            if (req == null) return NotFound();

            _context.VenueStaffingRequirement.Remove(req);
            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(req.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = req.DepartmentId });
        }
    }
}
