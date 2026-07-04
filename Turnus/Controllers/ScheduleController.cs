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
            // Parse week parameter or default to current week
            var today = DateTime.Today;
            DateTime weekStart;

            if (!string.IsNullOrEmpty(week))
            {
                // Parse ISO week format: 2026-W28
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

            // Navigation limits — ±6 months
            var minWeekStart = GetMonday(today.AddMonths(-6));
            var maxWeekStart = GetMonday(today.AddMonths(6));

            var currentUserId = _userManager.GetUserId(User);

            // Load all scheduled shifts for the week with full include chain
            var scheduledShifts = await _context.ScheduledShift
                .Include(s => s.ScheduledDay)
                    .ThenInclude(d => d.Venue)
                .Include(s => s.ShiftDefinition)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(a => a.Role)
                .Where(s => s.ScheduledDay.Date >= weekStart && s.ScheduledDay.Date <= weekEnd)
                .ToListAsync();

            // Load scheduled days for the week (for day assignments)
            var scheduledDayIds = scheduledShifts
                .Select(s => s.ScheduledDayId)
                .Distinct()
                .ToList();

            var scheduledDays = await _context.ScheduledDay
                .Include(d => d.Venue)
                .Include(d => d.DayAssignments)
                    .ThenInclude(a => a.Employee)
                .Include(d => d.DayAssignments)
                    .ThenInclude(a => a.Role)
                .Where(d => scheduledDayIds.Contains(d.Id))
                .ToListAsync();

            // Load staffing requirements for venues in this week
            var venueIds = scheduledDays.Select(d => d.VenueId).Distinct().ToList();
            var requirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => venueIds.Contains(r.VenueId))
                .ToListAsync();

            // Load current user's availability for this week's shifts
            var shiftIds = scheduledShifts.Select(s => s.Id).ToList();
            var myAvailability = await _context.Availability
                .Where(a => a.EmployeeId == currentUserId && shiftIds.Contains(a.ScheduledShiftId))
                .ToListAsync();

            // Load all venues for selector
            var venues = await _context.Venue.ToListAsync();

            // Build view model
            var viewModel = new WeekScheduleViewModel
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                WeekNumber = ISOWeek.GetWeekOfYear(weekStart),
                Year = weekStart.Year,
                CurrentUserId = currentUserId!,
                ScheduledShifts = scheduledShifts,
                ScheduledDays = scheduledDays,
                Requirements = requirements,
                MyAvailability = myAvailability,
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
        .Include(s => s.ScheduledDay)
            .ThenInclude(d => d.Venue)
        .Include(s => s.ShiftDefinition)
        .Include(s => s.ShiftAssignments)
            .ThenInclude(a => a.Employee)
        .Include(s => s.ShiftAssignments)
            .ThenInclude(a => a.Role)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (shift == null) return NotFound();

    var requirements = await _context.VenueStaffingRequirement
        .Include(r => r.Role)
        .Where(r => r.VenueId == shift.ScheduledDay.VenueId && r.IsShiftScoped)
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
            public List<ScheduledDay> ScheduledDays { get; set; } = new();
            public List<VenueStaffingRequirement> Requirements { get; set; } = new();
            public List<Availability> MyAvailability { get; set; } = new();
            public List<Venue> Venues { get; set; } = new();
            public bool CanGoPrevious { get; set; }
            public bool CanGoNext { get; set; }
            public string PreviousWeek { get; set; } = string.Empty;
            public string NextWeek { get; set; } = string.Empty;
            public string CurrentWeek { get; set; } = string.Empty;

            // Get all days of the week as a list
            public List<DateTime> WeekDays =>
                Enumerable.Range(0, 7).Select(i => WeekStart.AddDays(i)).ToList();

            // Get shifts for a specific day
            public List<ScheduledShift> ShiftsForDay(DateTime date) =>
                ScheduledShifts.Where(s => s.ScheduledDay.Date == date).ToList();

            // Get scheduled day for a specific date
            public ScheduledDay? DayFor(DateTime date) =>
                ScheduledDays.FirstOrDefault(d => d.Date == date);

            // Get the current user's shift assignments for a specific day
            public List<ShiftAssignment> MyShiftAssignmentsForDay(DateTime date) =>
                ScheduledShifts
                    .Where(s => s.ScheduledDay.Date == date)
                    .SelectMany(s => s.ShiftAssignments.Where(a => a.EmployeeId == CurrentUserId))
                    .ToList();

            // Get the current user's day assignments for a specific day
            public List<DayAssignment> MyDayAssignmentsForDay(DateTime date) =>
                ScheduledDays
                    .Where(d => d.Date == date)
                    .SelectMany(d => d.DayAssignments.Where(a => a.EmployeeId == CurrentUserId))
                    .ToList();

            // Get all employees who have any assignment this week (excluding current user)
            public List<ApplicationUser> OtherEmployees() =>
                ScheduledShifts
                    .SelectMany(s => s.ShiftAssignments)
                    .Select(a => a.Employee!)
                    .Concat(ScheduledDays.SelectMany(d => d.DayAssignments).Select(a => a.Employee!))
                    .Where(e => e.Id != CurrentUserId)
                    .DistinctBy(e => e.Id)
                    .ToList();

            // Get shift assignments for a specific employee on a specific day
            public List<ShiftAssignment> ShiftAssignmentsForEmployeeOnDay(string employeeId, DateTime date) =>
                ScheduledShifts
                    .Where(s => s.ScheduledDay.Date == date)
                    .SelectMany(s => s.ShiftAssignments.Where(a => a.EmployeeId == employeeId))
                    .ToList();

            // Get day assignments for a specific employee on a specific day
            public List<DayAssignment> DayAssignmentsForEmployeeOnDay(string employeeId, DateTime date) =>
                ScheduledDays
                    .Where(d => d.Date == date)
                    .SelectMany(d => d.DayAssignments.Where(a => a.EmployeeId == employeeId))
                    .ToList();

            // Check if a shift has open slots (assigned < required)
            public bool ShiftHasOpenSlots(ScheduledShift shift, List<VenueStaffingRequirement> requirements)
            {
                var venueId = shift.ScheduledDay.VenueId;
                foreach (var req in requirements.Where(r => r.VenueId == venueId && r.IsShiftScoped))
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