using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
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