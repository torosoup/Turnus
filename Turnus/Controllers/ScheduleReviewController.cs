using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize]
    public class ScheduleReviewController : Controller
    {
        private readonly TurnusContext _context;

        public ScheduleReviewController(TurnusContext context)
        {
            _context = context;
        }

        // GET: /ScheduleReview/Review?venueId=1&date=2026-07-14
        public async Task<IActionResult> Review(int venueId, DateTime date)
        {
            var venue = await _context.Venue.FindAsync(venueId);
            if (venue == null) return NotFound();

            var shifts = await _context.ScheduledShift
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s => s.VenueId == venueId && s.Date.Date == date.Date)
                .ToListAsync();

            var shiftIds = shifts.Select(s => s.Id).ToList();

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
                .Where(a => shiftIds.Contains(a.ScheduledShiftId) && a.IsAvailable)
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

            return View(shifts);
        }

        [HttpPost]
        public async Task<IActionResult> AssignShift(int scheduledShiftId, string employeeId, int roleId)
        {
            var alreadyAssigned = await _context.ShiftAssignment
                .AnyAsync(a => a.ScheduledShiftId == scheduledShiftId
                            && a.EmployeeId == employeeId
                            && a.RoleId == roleId);

            if (!alreadyAssigned)
            {
                _context.ShiftAssignment.Add(new ShiftAssignment
                {
                    ScheduledShiftId = scheduledShiftId,
                    EmployeeId = employeeId,
                    RoleId = roleId
                });
                await _context.SaveChangesAsync();
            }

            var shift = await _context.ScheduledShift.FindAsync(scheduledShiftId);
            return RedirectToAction(nameof(Review), new
            {
                venueId = shift!.VenueId,
                date = shift.Date.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        public async Task<IActionResult> UnassignShift(int shiftAssignmentId, int venueId, DateTime date)
        {
            var assignment = await _context.ShiftAssignment.FindAsync(shiftAssignmentId);
            if (assignment != null)
            {
                _context.ShiftAssignment.Remove(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Review), new
            {
                venueId,
                date = date.ToString("yyyy-MM-dd")
            });
        }
    }
}