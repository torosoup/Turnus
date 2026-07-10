using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleController(TurnusContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? week = null)
        {
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

            var scheduledShifts = await _context.ScheduledShift
                .Include(s => s.Venue)
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s => s.Date >= weekStart && s.Date <= weekEnd)
                .ToListAsync();

            var departmentIds = scheduledShifts
                .Where(s => s.DepartmentId.HasValue)
                .Select(s => s.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => departmentIds.Contains(r.DepartmentId))
                .ToListAsync();

            var venues = await _context.Venue.ToListAsync();

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

            var shift = await _context.ScheduledShift
                .Include(s => s.Venue)
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null) return NotFound();

            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => shift.DepartmentId != null && r.DepartmentId == shift.DepartmentId)
                .ToListAsync();

            var myAvailability = await _context.Availability
                .FirstOrDefaultAsync(a => a.EmployeeId == currentUserId && a.ScheduledShiftId == id);

            ViewBag.Requirements = requirements;
            ViewBag.MyAvailability = myAvailability;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ReturnWeek = week ?? string.Empty;

            return View(shift);
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
                foreach (var req in requirements.Where(r => r.IsShiftScoped))
                {
                    var assigned = shift.ShiftAssignments.Count(a => a.RoleId == req.RoleId);
                    if (assigned < req.RequiredCount) return true;
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
    }
}