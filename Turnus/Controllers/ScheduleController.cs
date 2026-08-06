using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceMember")]
    public class ScheduleController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public ScheduleController(TurnusContext context, UserManager<ApplicationUser> userManager, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _userManager = userManager;
            _workspaceProvider = workspaceProvider;
        }

        public async Task<IActionResult> Index(string? week = null, int? venueId = null, int? departmentId = null)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var today = DateTime.Today;
            DateTime weekStart;

            if (!string.IsNullOrEmpty(week))
            {
                try
                {
                    var parts = week.Split("-W");
                    int year = int.Parse(parts[0]);
                    int weekNumber = int.Parse(parts[1]);
                    weekStart = ISOWeekStart(year, weekNumber);
                }
                catch
                {
                    weekStart = GetMonday(today);
                }
            }
            else
            {
                weekStart = GetMonday(today);
            }

            var weekEnd = weekStart.AddDays(6);

            var minWeekStart = GetMonday(today.AddMonths(-6));
            var maxWeekStart = GetMonday(today.AddMonths(6));

            var currentUserId = _userManager.GetUserId(User);

            var scheduledShiftsQuery = _context.ScheduledShift
                .Include(s => s.Venue)
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s => s.Date.Date >= weekStart.Date &&
                            s.Date.Date <= weekEnd.Date &&
                            s.WorkspaceId == wsId.Value);

            // Load venues early so we can default to the first venue when none specified
            var venues = await _context.Venue
                .Where(v => v.WorkspaceId == wsId.Value)
                .ToListAsync();

            if (!venueId.HasValue && venues.Any())
            {
                venueId = venues.First().Id;
            }

            if (venueId.HasValue)
            {
                scheduledShiftsQuery = scheduledShiftsQuery
                    .Where(s => s.VenueId == venueId.Value);
            }

            if (departmentId.HasValue)
            {
                scheduledShiftsQuery = scheduledShiftsQuery
                    .Where(s => s.DepartmentId == departmentId.Value);
            }

            var scheduledShifts = await scheduledShiftsQuery.ToListAsync();

            var departmentIds = scheduledShifts
                .Where(s => s.DepartmentId.HasValue)
                .Select(s => s.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => departmentIds.Contains(r.DepartmentId) && r.WorkspaceId == wsId.Value)
                .ToListAsync();

            // Load departments for the selected venue for the department filter
            var departmentsForVenue = new List<Department>();
            if (venueId.HasValue)
            {
                departmentsForVenue = await _context.Department
                    .Where(d => d.VenueId == venueId.Value && d.WorkspaceId == wsId.Value)
                    .ToListAsync();
            }

            var viewModel = new WeekScheduleViewModel
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                WeekNumber = ISOWeek.GetWeekOfYear(weekStart),
                Year = weekStart.Year,
                CurrentUserId = currentUserId!,
                ScheduledShifts = scheduledShifts,
                Requirements = requirements,
                Venues = venues,
                Departments = departmentsForVenue,
                SelectedVenueId = venueId ?? 0,
                SelectedDepartmentId = departmentId,
                CanGoPrevious = weekStart > minWeekStart,
                CanGoNext = weekStart < maxWeekStart,
                PreviousWeek = FormatWeek(weekStart.AddDays(-7)),
                NextWeek = FormatWeek(weekStart.AddDays(7)),
                CurrentWeek = FormatWeek(weekStart)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ShiftDetail(int id, string? week = null)
        {
            var currentUserId = _userManager.GetUserId(User);

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var shift = await _context.ScheduledShift
                .Include(s => s.Venue)
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(s => s.Id == id && s.WorkspaceId == wsId.Value);

            if (shift == null)
                return NotFound();

            //  throw new Exception($"Shift ID: {shift.Id}, DepartmentId: {shift.DepartmentId}");

            // Console.WriteLine($"Shift ID: {shift.Id}");
            // Console.WriteLine($"Shift DepartmentId: {shift.DepartmentId}");

            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => shift.DepartmentId != null &&
                            r.DepartmentId == shift.DepartmentId &&
                            r.WorkspaceId == wsId.Value)
                .ToListAsync();

            // Console.WriteLine($"Requirements found: {requirements.Count}");

            var myAvailability = await _context.Availability
                .FirstOrDefaultAsync(a =>
                    a.EmployeeId == currentUserId &&
                    a.ScheduledShiftId == id &&
                    a.WorkspaceId == wsId.Value);

            ViewBag.Requirements = requirements;
            ViewBag.MyAvailability = myAvailability;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ReturnWeek = week ?? "";

            return PartialView("~/Views/Schedule/Partial/_ShiftDetail.cshtml", shift);
        }

        // Nested view model — not a separate model file
        public class WeekScheduleViewModel
        {
            public DateTime WeekStart { get; set; }
            public DateTime WeekEnd { get; set; }
            public int WeekNumber { get; set; }
            public int Year { get; set; }
            public string CurrentUserId { get; set; } = string.Empty;
            public List<ScheduledShift> ScheduledShifts { get; set; } = new();
            public List<VenueStaffingRequirement> Requirements { get; set; } = new();
            public List<Venue> Venues { get; set; } = new();
            public List<Department> Departments { get; set; } = new();
            public int SelectedVenueId { get; set; }
            public int? SelectedDepartmentId { get; set; }
            public bool CanGoPrevious { get; set; }
            public bool CanGoNext { get; set; }
            public string PreviousWeek { get; set; } = string.Empty;
            public string NextWeek { get; set; } = string.Empty;
            public string CurrentWeek { get; set; } = string.Empty;

            public List<DateTime> WeekDays =>
                Enumerable.Range(0, 7).Select(i => WeekStart.AddDays(i)).ToList();

            public List<ScheduledShift> ShiftsForDay(DateTime date) =>
                ScheduledShifts.Where(s => s.Date.Date == date.Date).ToList();

            public List<ShiftAssignment> MyShiftAssignmentsForDay(DateTime date) =>
                ScheduledShifts
                    .Where(s => s.Date.Date == date.Date)
                    .SelectMany(s => s.ShiftAssignments.Where(a => a.EmployeeId == CurrentUserId))
                    .ToList();

            public List<ApplicationUser> OtherEmployees() =>
                ScheduledShifts
                    .SelectMany(s => s.ShiftAssignments)
                    .Select(a => a.Employee!)
                    .Where(e => e != null && e.Id != CurrentUserId)
                    .DistinctBy(e => e.Id)
                    .ToList();

            public List<ShiftAssignment> ShiftAssignmentsForEmployeeOnDay(string employeeId, DateTime date) =>
                ScheduledShifts
                    .Where(s => s.Date.Date == date.Date)
                    .SelectMany(s => s.ShiftAssignments.Where(a => a.EmployeeId == employeeId))
                    .ToList();

            public bool ShiftHasOpenSlots(ScheduledShift shift, List<VenueStaffingRequirement> requirements)
            {
                // Consider both shift-scoped and day-scoped requirements.
                // Shift is considered to have open slots if any requirement (shift- or day-scoped)
                // has fewer assigned employees than RequiredCount.

                // If there are no requirements for this department, return false (no open slots to satisfy)
                var relatedRequirements = requirements
                    .Where(r => r.DepartmentId == shift.DepartmentId)
                    .ToList();

                if (!relatedRequirements.Any())
                    return false;

                foreach (var req in relatedRequirements)
                {
                    if (req.IsShiftScoped)
                    {
                        // Count assignments for this specific scheduled shift
                        var assigned = shift.ShiftAssignments.Count(a => a.RoleId == req.RoleId);
                        if (assigned < req.RequiredCount)
                            return true;
                    }
                    else
                    {
                        // Day-scoped: aggregate assignments across all scheduled shifts for the same day/department/venue
                        var assignedDay = ScheduledShifts
                            .Where(s => s.Date.Date == shift.Date.Date
                                        && s.DepartmentId == shift.DepartmentId
                                        && s.VenueId == shift.VenueId)
                            .SelectMany(s => s.ShiftAssignments)
                            .Count(a => a.RoleId == req.RoleId);

                        if (assignedDay < req.RequiredCount)
                            return true;
                    }
                }

                return false;
            }
        }

        private static DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private static DateTime ISOWeekStart(int year, int week)
        {
            var jan4 = new DateTime(year, 1, 4);
            var startOfFirstWeek = GetMonday(jan4);
            return startOfFirstWeek.AddDays((week - 1) * 7);
        }

        private static string FormatWeek(DateTime monday)
        {
            return $"{monday.Year}-W{ISOWeek.GetWeekOfYear(monday):D2}";
        }

        [HttpPost]
        public async Task<IActionResult> SetAvailability(int scheduledShiftId, bool isAvailable, string? week = null)
        {
            var userId = _userManager.GetUserId(User);

            var existing = await _context.Availability
                .FirstOrDefaultAsync(a =>
                    a.EmployeeId == userId &&
                    a.ScheduledShiftId == scheduledShiftId);

            if (existing != null)
            {
                existing.IsAvailable = isAvailable;
            }
            else
            {
                _context.Availability.Add(new Availability
                {
                    EmployeeId = userId!,
                    ScheduledShiftId = scheduledShiftId,
                    IsAvailable = isAvailable
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Schedule", new { week = week });
        }
    }
}