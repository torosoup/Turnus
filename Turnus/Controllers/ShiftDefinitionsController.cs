using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    public class ShiftDefinitionsController : Controller
    {
        private readonly TurnusContext _context;

        public ShiftDefinitionsController(TurnusContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Department.ToListAsync();

            return PartialView(
                "~/Views/Admin/Partials/Configuration/ShiftDefinition/_CreateShiftDefinition.cshtml",
                new ShiftDefinition());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShiftDefinition model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Department.ToListAsync();

                return PartialView(
                    "~/Views/Admin/Partials/Configuration/ShiftDefinition/_CreateShiftDefinition.cshtml",
                    model);
            }

            _context.ShiftDefinition.Add(model);
            await _context.SaveChangesAsync();

            // Preserve dashboard context: department + venue if available
            var dept = await _context.Department.FindAsync(model.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = model.DepartmentId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sd = await _context.ShiftDefinition.FindAsync(id);

            if (sd == null)
            {
                return NotFound();
            }

            ViewBag.Departments = await _context.Department.ToListAsync();

            return PartialView(
                "~/Views/Admin/Partials/Configuration/ShiftDefinition/_EditShiftDefinition.cshtml",
                sd);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ShiftDefinition model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Department.ToListAsync();

                return PartialView(
                    "~/Views/Admin/Partials/Configuration/ShiftDefinition/_EditShiftDefinition.cshtml",
                    model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(model.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = model.DepartmentId });
        }

        public async Task<IActionResult> Delete(int id)
        {

            var sd = await _context.ShiftDefinition.FindAsync(id);

            if (sd == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/Admin/Partials/Configuration/ShiftDefinition/_DeleteShiftDefinition.cshtml", sd);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sd = await _context.ShiftDefinition
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sd == null)
                return NotFound();

            var scheduledShifts = await _context.ScheduledShift
                .Where(x => x.ShiftDefinitionId == id)
                .ToListAsync();

            _context.ScheduledShift.RemoveRange(scheduledShifts);
            _context.ShiftDefinition.Remove(sd);

            await _context.SaveChangesAsync();

            var dept = await _context.Department.FindAsync(sd.DepartmentId);
            return RedirectToAction("Dashboard", "Admin", new { venueId = dept?.VenueId, departmentId = sd.DepartmentId });
        }
    }
}
