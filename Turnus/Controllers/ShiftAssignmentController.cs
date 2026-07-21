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
                // Derive scheduled shift information from the server-side record
                var scheduledShift = await _context.ScheduledShift
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduledShiftId);

                if (scheduledShift == null)
                {
                    return BadRequest();
                }

                // Find the staffing requirement matching the role + department (the UI filters requirements by DepartmentId)
                var requirement = await _context.VenueStaffingRequirement
                    .FirstOrDefaultAsync(r =>
                        r.RoleId == model.RoleId &&
                        r.DepartmentId == scheduledShift.DepartmentId);

                if (requirement != null)
                {
                    int assignedCount;

                    if (requirement.IsShiftScoped)
                    {
                        // Per-shift enforcement: count assignments for this scheduled shift + role
                        assignedCount = await _context.ShiftAssignment
                            .CountAsync(a =>
                                a.ScheduledShiftId == model.ScheduledShiftId &&
                                a.RoleId == model.RoleId);
                    }
                    else
                    {
                        // Day-scoped enforcement: aggregate across all shifts for the same venue + department + date
                        var shiftIds = await _context.ScheduledShift
                            .Where(s =>
                                s.VenueId == scheduledShift.VenueId &&
                                s.DepartmentId == scheduledShift.DepartmentId &&
                                s.Date.Date == scheduledShift.Date.Date)
                            .Select(s => s.Id)
                            .ToListAsync();

                        assignedCount = await _context.ShiftAssignment
                            .CountAsync(a => shiftIds.Contains(a.ScheduledShiftId) && a.RoleId == model.RoleId);
                    }

                    if (assignedCount >= requirement.RequiredCount)
                    {
                        ModelState.AddModelError("", "This role is already fully staffed.");
                        return BadRequest(ModelState);
                    }
                }

                _context.ShiftAssignment.Add(model);
                await _context.SaveChangesAsync();
            }

            /* return RedirectToAction(
                "Review",
                "ScheduleReview",
                new
                {
                    venueId,
                    date = date.ToString("yyyy-MM-dd")
                }); */

            return await ReturnScheduleReview(venueId, date);
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

            /* return RedirectToAction(
                "Review",
                "ScheduleReview",
                new
                {
                    venueId,
                    date = date.ToString("yyyy-MM-dd")
                }); */

            return await ReturnScheduleReview(venueId, date);


        }

        private async Task<IActionResult> ReturnScheduleReview(int venueId, DateTime date)
        {
            var venue = await _context.Venue.FindAsync(venueId);

            if (venue == null)
                return NotFound();

            var shifts = await _context.ScheduledShift
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s =>
                    s.VenueId == venueId &&
                    s.Date.Date == date.Date)
                .ToListAsync();

            var shiftIds = shifts
                .Select(s => s.Id)
                .ToList();

            var departmentIds = shifts
                .Where(s => s.DepartmentId.HasValue)
                .Select(s => s.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => departmentIds.Contains(r.DepartmentId))
                .ToListAsync();

            var availability = await _context.Availability
                .Where(a =>
                    shiftIds.Contains(a.ScheduledShiftId) &&
                    a.IsAvailable)
                .Include(a => a.Employee)
                .ToListAsync();

            var allEmployees = await _context.Users
                .Cast<ApplicationUser>()
                .ToListAsync();

            ViewBag.Venue = venue;
            ViewBag.Date = date;
            ViewBag.Requirements = requirements;
            ViewBag.Availability = availability;
            ViewBag.AllEmployees = allEmployees;

            return PartialView(
                "~/Views/Schedule/Partial/_ScheduleReview.cshtml",
                shifts);
        }
    }
}