using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Roles = "Manager")]
    public class AdminController : Controller
    {
        private readonly TurnusContext _context;

        public AdminController(TurnusContext context)
        {
            _context = context;
        }

        // ---------------------------
        // MAIN DASHBOARD VIEW
        // ---------------------------
        public async Task<IActionResult> Dashboard(int? venueId)
        {
            var venues = await _context.Venue.ToListAsync();

            var selectedVenue = venueId.HasValue
                ? venues.FirstOrDefault(v => v.Id == venueId.Value)
                : venues.FirstOrDefault();

            var model = await BuildDashboardModel(selectedVenue?.Id ?? 0);
            model.Venues = venues;
            model.SelectedVenue = selectedVenue;

            return View(model);
        }

        // ---------------------------
        // PARTIAL SECTIONS
        // ---------------------------
        public async Task<IActionResult> VenueSettingsSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("_VenueSettingsSection", model);
        }

        public async Task<IActionResult> DepartmentsSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("_DepartmentsSection", model);
        }

        public async Task<IActionResult> RolesSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("_RolesSection", model);
        }

        public async Task<IActionResult> ShiftDefinitionsSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("_ShiftDefinitionsSection", model);
        }

        public async Task<IActionResult> StaffingRequirementsSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("_StaffingRequirementsSection", model);
        }

        public async Task<IActionResult> ScheduleSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("_ScheduleSection", model);
        }

        // ---------------------------
        // SHARED MODEL BUILDER
        // ---------------------------
        private async Task<DashboardViewModel> BuildDashboardModel(int venueId)
        {
            var venues = await _context.Venue.ToListAsync();
            var selectedVenue = venues.FirstOrDefault(v => v.Id == venueId);

            var roles = await _context.Role.ToListAsync();
            var departments = await _context.Department.Where(d => d.VenueId == venueId).ToListAsync();
            var departmentIds = departments.Select(d => d.Id).ToList();

            var shiftDefinitions = await _context.ShiftDefinition
                .Include(s => s.Department)
                .Where(s => departmentIds.Contains(s.DepartmentId))
                .ToListAsync();

            var staffingRequirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Include(r => r.Department)
                .Where(r => departmentIds.Contains(r.DepartmentId))
                .ToListAsync();

            var today = DateTime.Today;
            var maxDate = today.AddMonths(6);

            var scheduledShifts = await _context.ScheduledShift
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Where(s => s.VenueId == venueId && s.Date >= today && s.Date <= maxDate)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var shiftsByDate = scheduledShifts
                .GroupBy(s => s.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            return new DashboardViewModel
            {
                Venues = venues,
                SelectedVenue = selectedVenue,
                Roles = roles,
                Departments = departments,
                ShiftDefinitions = shiftDefinitions,
                StaffingRequirements = staffingRequirements,
                ShiftsByDate = shiftsByDate,
                HasRoles = roles.Any(),
                HasDepartments = departments.Any(),
                HasShiftDefinitions = shiftDefinitions.Any(),
                HasStaffingRequirements = staffingRequirements.Any(),
                VenueSettingsComplete = roles.Any() && departments.Any() && shiftDefinitions.Any() && staffingRequirements.Any()
            };
        }

        // ---------------------------
        // YOUR EXISTING INNER MODEL
        // ---------------------------
        public class DashboardViewModel
        {
            public List<Venue> Venues { get; set; } = new();
            public Venue? SelectedVenue { get; set; }
            public List<Role> Roles { get; set; } = new();
            public List<Department> Departments { get; set; } = new();
            public List<ShiftDefinition> ShiftDefinitions { get; set; } = new();
            public List<VenueStaffingRequirement> StaffingRequirements { get; set; } = new();
            public List<IGrouping<DateTime, ScheduledShift>> ShiftsByDate { get; set; } = new();
            public bool HasRoles { get; set; }
            public bool HasDepartments { get; set; }
            public bool HasShiftDefinitions { get; set; }
            public bool HasStaffingRequirements { get; set; }
            public bool VenueSettingsComplete { get; set; }
        }
    }
}
