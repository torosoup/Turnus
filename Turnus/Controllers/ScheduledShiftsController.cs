using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class ScheduledShiftsController : Controller
    {
        private readonly TurnusContext _context;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public ScheduledShiftsController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
        }

        public async Task<IActionResult> Create(int venueId, int departmentId, DateTime date)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var shiftDefs = await _context.ShiftDefinition
                .Where(s => s.DepartmentId == departmentId && s.WorkspaceId == wsId.Value)
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
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            // Verify department exists and belongs to the selected workspace
            if (model.DepartmentId != null)
            {
                var dept = await _context.Department.FindAsync(model.DepartmentId);
                if (dept == null || dept.WorkspaceId != wsId.Value)
                {
                    ModelState.AddModelError("DepartmentId", "Selected department is invalid for the chosen workspace.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ShiftDefinitions = await _context.ShiftDefinition.Where(s => s.WorkspaceId == wsId.Value).ToListAsync();
                return PartialView("~/Views/Admin/Partials/ScheduleManagement/ScheduledShift/_CreateScheduledShift.cshtml", model);
            }

            model.WorkspaceId = wsId.Value;
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
