using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ScheduledShiftsController : Controller
    {
        private readonly TurnusContext _context;

        public ScheduledShiftsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int venueId, int departmentId, DateTime date)
        {
            var shiftDefs = await _context.ShiftDefinition
                .Where(s => s.DepartmentId == departmentId)
                .ToListAsync();

            ViewBag.ShiftDefinitions = shiftDefs;

            return PartialView(
                "~/Views/Admin/Partials/ScheduleManagement/ScheduledShift/_CreateScheduledShift.cshtml",
                new ScheduledShift
                {
                    VenueId = venueId,
                    DepartmentId = departmentId,
                    Date = date
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduledShift model)
        {
            // Verify department exists and belongs to the selected venue
            if (model.DepartmentId != null)
            {
                var dept = await _context.Department.FindAsync(model.DepartmentId);
                if (dept == null || dept.VenueId != model.VenueId)
                {
                    ModelState.AddModelError("DepartmentId", "Selected department is invalid for the chosen venue.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ShiftDefinitions = await _context.ShiftDefinition.ToListAsync();
                return PartialView("~/Views/Admin/Partials/ScheduleManagement/ScheduledShift/_CreateScheduledShift.cshtml", model);
            }

            _context.ScheduledShift.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin", new { venueId = model.VenueId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.ScheduledShift
                .Include(s => s.ShiftDefinition)
                .FirstOrDefaultAsync(s => s.Id == id);

            return PartialView("~/Views/Admin/Partials/ScheduleManagement/ScheduledShift/_DeleteScheduledShift.cshtml", shift);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ScheduledShift model)
        {
            var shift = await _context.ScheduledShift.FindAsync(model.Id);
            if (shift == null) return NotFound();

            _context.ScheduledShift.Remove(shift);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Admin", new { venueId = shift.VenueId });
        }
    }
}
