using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    public class StaffingRequirementsController : Controller
    {
        private readonly TurnusContext _context;

        public StaffingRequirementsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int departmentId)
        {
            ViewBag.Roles = await _context.Role.ToListAsync();
            return PartialView("~/Views/Admin/Partials/StaffingRequirement/_CreateStaffingRequirement.cshtml",
                new VenueStaffingRequirement { DepartmentId = departmentId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(VenueStaffingRequirement model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Role.ToListAsync();
                return PartialView("~/Views/Admin/Partials/StaffingRequirement/_CreateStaffingRequirement.cshtml", model);
            }

            _context.VenueStaffingRequirement.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var req = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id);

            ViewBag.Roles = await _context.Role.ToListAsync();

            return PartialView("~/Views/Admin/Partials/StaffingRequirement/_EditStaffingRequirement.cshtml", req);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VenueStaffingRequirement model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Role.ToListAsync();
                return PartialView("~/Views/Admin/Partials/StaffingRequirement/_EditStaffingRequirement.cshtml", model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var req = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id);

            return PartialView("~/Views/Admin/Partials/StaffingRequirement/_DeleteStaffingRequirement.cshtml", req);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(VenueStaffingRequirement model)
        {
            var req = await _context.VenueStaffingRequirement.FindAsync(model.Id);
            _context.VenueStaffingRequirement.Remove(req);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
