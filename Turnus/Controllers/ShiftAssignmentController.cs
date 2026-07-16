using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ShiftAssignmentController : Controller
    {
        private readonly TurnusContext _context;

        public ShiftAssignmentController(TurnusContext context)
        {
            _context = context;
        }

        // -------------------------------------------------
        // CREATE MODAL
        // -------------------------------------------------

        public async Task<IActionResult> Create(
            int scheduledShiftId,
            int roleId,
            int venueId,
            DateTime date,
            string? employeeId = null)
        {
            ViewBag.AllEmployees = await _context.Users
                .Cast<ApplicationUser>()
                .ToListAsync();

            ViewBag.Roles = await _context.Role
                .Where(r => r.Id == roleId)
                .ToListAsync();

            ViewBag.VenueId = venueId;
            ViewBag.Date = date;

            return PartialView(
                "~/Views/Admin/Partials/ScheduleManagement/ShiftAssignment/_AssignShift.cshtml",
                new ShiftAssignment
                {
                    ScheduledShiftId = scheduledShiftId,
                    RoleId = roleId,
                    EmployeeId = employeeId
                });
        }

        // -------------------------------------------------
        // ASSIGN
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShift(
            ShiftAssignment model,
            int venueId,
            DateTime date)
        {
            if (!await _context.ShiftAssignment.AnyAsync(a =>
                    a.ScheduledShiftId == model.ScheduledShiftId &&
                    a.EmployeeId == model.EmployeeId &&
                    a.RoleId == model.RoleId))
            {
                var requirement = await _context.VenueStaffingRequirement
                    .FirstOrDefaultAsync(r =>
                        r.RoleId == model.RoleId &&
                        r.IsShiftScoped);

                if (requirement != null)
                {
                    var assignedCount = await _context.ShiftAssignment
                        .CountAsync(a =>
                            a.ScheduledShiftId == model.ScheduledShiftId &&
                            a.RoleId == model.RoleId);

                    if (assignedCount >= requirement.RequiredCount) // 
                    {
                        ModelState.AddModelError("", "This role is already fully staffed.");
                        return BadRequest(ModelState);
                    }
                }

                _context.ShiftAssignment.Add(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Review",
                "ScheduleReview",
                new
                {
                    venueId,
                    date = date.ToString("yyyy-MM-dd")
                });
        }

        // -------------------------------------------------
        // DELETE MODAL
        // -------------------------------------------------

        public async Task<IActionResult> Delete(
            int id,
            int venueId,
            DateTime date)
        {
            var assignment = await _context.ShiftAssignment
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound();

            ViewBag.VenueId = venueId;
            ViewBag.Date = date;

            return PartialView(
                "~/Views/Admin/Partials/ScheduleManagement/ShiftAssignment/_UnassignShift.cshtml",
                assignment);
        }

        // -------------------------------------------------
        // UNASSIGN
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignShift(
            int id,
            int venueId,
            DateTime date)
        {
            var assignment = await _context.ShiftAssignment.FindAsync(id);

            if (assignment != null)
            {
                _context.ShiftAssignment.Remove(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Review",
                "ScheduleReview",
                new
                {
                    venueId,
                    date = date.ToString("yyyy-MM-dd")
                });
        }
    }
}