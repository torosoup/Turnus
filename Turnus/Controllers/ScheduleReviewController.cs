using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class ScheduleReviewController : Controller
    {
        private readonly TurnusContext _context;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public ScheduleReviewController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
        }

        // GET: /ScheduleReview/Review?venueId=1&date=2026-07-14
        public async Task<IActionResult> Review(int venueId, DateTime date)
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

            // Only include users who are members of the workspace
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

            return PartialView(
                "~/Views/Schedule/Partial/_ScheduleReview.cshtml",
                shifts);
        }

        /*
        private async Task<IActionResult> LoadReviewPartial(
            int venueId,
            DateTime date)
        {
            var venue = await _context.Venue.FindAsync(venueId);

            var shifts = await _context.ScheduledShift
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s =>
                    s.VenueId == venueId &&
                    s.Date >= date.Date &&
                    s.Date < date.Date.AddDays(1))
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
        } */
    }
}