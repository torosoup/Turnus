using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;
using Microsoft.Data.SqlClient;
using Turnus.Services;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class ShiftAssignmentController : Controller
    {
        private readonly TurnusContext _context;
        private readonly ICurrentWorkspaceProvider _workspaceProvider;

        public ShiftAssignmentController(TurnusContext context, ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
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
            // Verify active workspace and membership
            var currentWorkspaceId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!currentWorkspaceId.HasValue) return Forbid();

            // Verify scheduled shift belongs to the current workspace
            var ssCheck = await _context.ScheduledShift.FindAsync(scheduledShiftId);
            if (ssCheck == null || ssCheck.WorkspaceId != currentWorkspaceId.Value) return NotFound();

            // Verify role belongs to current workspace
            var roleCheck = await _context.Role.FindAsync(roleId);
            if (roleCheck == null || roleCheck.WorkspaceId != currentWorkspaceId.Value) return NotFound();

            ViewBag.AllEmployees = await _context.Users
                .Cast<ApplicationUser>()
                .ToListAsync();

            ViewBag.Roles = await _context.Role
                .Where(r => r.Id == roleId && r.WorkspaceId == currentWorkspaceId.Value)
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
            var currentWorkspaceId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!currentWorkspaceId.HasValue) return Forbid();

            if (!await _context.ShiftAssignment.AnyAsync(a =>
                    a.ScheduledShiftId == model.ScheduledShiftId &&
                    a.EmployeeId == model.EmployeeId && 
                    a.RoleId == model.RoleId))
            {
                // Derive scheduled shift information from the server-side record
                var scheduledShift = await _context.ScheduledShift
                    .Include(s => s.ShiftDefinition)
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduledShiftId);

                if (scheduledShift == null)
                {
                    return BadRequest();
                }

                // Ensure scheduled shift is in the current workspace
                if (scheduledShift.WorkspaceId != currentWorkspaceId.Value) return Forbid();

                // Ensure the role belongs to the current workspace
                var role = await _context.Role.FindAsync(model.RoleId);
                if (role == null || role.WorkspaceId != currentWorkspaceId.Value) return Forbid();

                // Find the staffing requirement matching the role + department (the UI filters requirements by DepartmentId)
                var requirement = await _context.VenueStaffingRequirement
                    .FirstOrDefaultAsync(r =>
                        r.RoleId == model.RoleId &&
                        r.DepartmentId == scheduledShift.DepartmentId &&
                        r.WorkspaceId == currentWorkspaceId.Value);

                // Prevent an employee being assigned more than one role for the same scheduled shift
                if (!string.IsNullOrEmpty(model.EmployeeId))
                {
                    var alreadyForThisShift = await _context.ShiftAssignment
                        .AnyAsync(a => a.ScheduledShiftId == model.ScheduledShiftId && a.EmployeeId == model.EmployeeId && a.WorkspaceId == currentWorkspaceId.Value);

                    if (alreadyForThisShift)
                    {
                        ModelState.AddModelError("", "This employee is already assigned to a role for this shift.");
                        return await ReturnScheduleReview(venueId, date);
                    }

                    // Prevent an employee being assigned to another shift that overlaps in time
                    // Load existing assignments for this employee (exclude current scheduled shift)
                    var existingAssignments = await _context.ShiftAssignment
                        .Include(a => a.ScheduledShift).ThenInclude(s => s.ShiftDefinition)
                        .Where(a => a.EmployeeId == model.EmployeeId && a.ScheduledShiftId != model.ScheduledShiftId && a.WorkspaceId == currentWorkspaceId.Value)
                        .ToListAsync();

                    if (scheduledShift?.ShiftDefinition != null)
                    {
                        var currentStart = scheduledShift.Date.Date + scheduledShift.ShiftDefinition.StartTime;
                        var currentEnd = scheduledShift.Date.Date + scheduledShift.ShiftDefinition.EndTime;

                        // Handle overnight shifts where EndTime is less than or equal to StartTime
                        if (scheduledShift.ShiftDefinition.EndTime <= scheduledShift.ShiftDefinition.StartTime)
                            currentEnd = currentEnd.AddDays(1);

                        foreach (var a in existingAssignments)
                        {
                            var s = a.ScheduledShift;
                            if (s?.ShiftDefinition == null) continue;

                            var otherStart = s.Date.Date + s.ShiftDefinition.StartTime;
                            var otherEnd = s.Date.Date + s.ShiftDefinition.EndTime;
                            if (s.ShiftDefinition.EndTime <= s.ShiftDefinition.StartTime)
                                otherEnd = otherEnd.AddDays(1);

                            // Overlap if start < otherEnd && otherStart < end
                            if (currentStart < otherEnd && otherStart < currentEnd)
                            {
                                ModelState.AddModelError("", "This employee is already assigned to another shift that overlaps this one.");
                                return await ReturnScheduleReview(venueId, date);
                            }
                        }
                    }
                }

                if (requirement != null)
                {
                    int assignedCount;

                    if (requirement.IsShiftScoped)
                    {
                        // Per-shift enforcement: count assignments for this scheduled shift + role
                        assignedCount = await _context.ShiftAssignment
                            .CountAsync(a =>
                                a.ScheduledShiftId == model.ScheduledShiftId &&
                                a.RoleId == model.RoleId &&
                                a.WorkspaceId == currentWorkspaceId.Value);
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
                        return await ReturnScheduleReview(venueId, date);
                    }
                }

                // Ensure the assignment is marked with the current workspace
                model.WorkspaceId = currentWorkspaceId.Value;

                _context.ShiftAssignment.Add(model);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Handle SQL unique constraint violations (duplicate key) gracefully
                    if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                    {
                        ModelState.AddModelError("", "This assignment already exists or a conflicting assignment was created concurrently.");
                        return await ReturnScheduleReview(venueId, date);
                    }

                    throw;
                }
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
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var venue = await _context.Venue.FindAsync(venueId);
            if (venue == null || venue.WorkspaceId != wsId.Value) return NotFound();

            var shifts = await _context.ScheduledShift
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s => s.VenueId == venueId && s.Date.Date == date.Date && s.WorkspaceId == wsId.Value)
                .ToListAsync();

            var shiftIds = shifts.Select(s => s.Id).ToList();

            var departmentIds = shifts
                .Where(s => s.DepartmentId.HasValue)
                .Select(s => s.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => departmentIds.Contains(r.DepartmentId) && r.WorkspaceId == wsId.Value)
                .ToListAsync();

            var availability = await _context.Availability
                .Where(a => shiftIds.Contains(a.ScheduledShiftId) && a.IsAvailable && a.WorkspaceId == wsId.Value)
                .Include(a => a.Employee)
                .ToListAsync();

            var userIds = await _context.WorkspaceMember
                .Where(wm => wm.WorkspaceId == wsId.Value)
                .Select(wm => wm.UserId)
                .ToListAsync();

            var allEmployees = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Cast<ApplicationUser>()
                .ToListAsync();

            ViewBag.Venue = venue;
            ViewBag.Date = date;
            ViewBag.Requirements = requirements;
            ViewBag.Availability = availability;
            ViewBag.AllEmployees = allEmployees;

            return PartialView("~/Views/Schedule/Partial/_ScheduleReview.cshtml", shifts);
        }
    }
}