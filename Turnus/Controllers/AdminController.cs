using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class AdminController : Controller
    {
        private readonly TurnusContext _context;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public AdminController(TurnusContext context, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _workspaceProvider = workspaceProvider;
        }

        // ---------------------------
        // MAIN DASHBOARD VIEW
        // ---------------------------
        public async Task<IActionResult> Dashboard(int? venueId, int? departmentId)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            // Venues are automatically filtered by the DbContext global query filters,
            // but we load them explicitly to drive UI selection.
            var venues = await _context.Venue.ToListAsync();

            // If there are no venues (fresh workspace), return an empty dashboard model so the
            // views and client-side scripts can render a friendly UI that lets the user
            // create the first venue.
            if (!venues.Any())
            {
                var emptyModel = new DashboardViewModel
                {
                    Venues = venues,
                    SelectedVenue = null,
                    Departments = new List<Department>(),
                    SelectedDepartment = null
                };

                return View(emptyModel);
            }

            var selectedVenue = venueId.HasValue
                ? venues.FirstOrDefault(v => v.Id == venueId.Value)
                : venues.FirstOrDefault();

            // Ensure we have a selected venue before querying departments.
            var departments = selectedVenue is not null
                ? await _context.Department.Where(d => d.VenueId == selectedVenue.Id).ToListAsync()
                : new List<Department>();

            var selectedDepartment = departments
                .FirstOrDefault(d => d.Id == departmentId)
                ?? departments.FirstOrDefault();

            var model = await BuildDashboardModel(
                selectedVenue?.Id ?? 0,
                selectedDepartment?.Id
            );

            model.Venues = venues;
            model.SelectedVenue = selectedVenue;
            model.Departments = departments;
            model.SelectedDepartment = selectedDepartment;

            return View(model);
        }

        // ---------------------------
        // PARTIAL SECTIONS
        // ---------------------------
        public async Task<IActionResult> VenueSettingsSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("Partials/Configuration/_VenueSettingsSection", model);
        }

        public async Task<IActionResult> DepartmentsSection(int venueId)
        {
            var model = await BuildDashboardModel(venueId);
            return PartialView("Partials/Configuration/_DepartmentsSection", model);
        }

        public async Task<IActionResult> RolesSection(int venueId, int? departmentId)
        {
            var model = await BuildDashboardModel(venueId, departmentId);
            return PartialView("Partials/Configuration/_RolesSection", model);
        }

        public async Task<IActionResult> ShiftDefinitionsSection(int venueId, int? departmentId)
        {
            var model = await BuildDashboardModel(venueId, departmentId);
            return PartialView("Partials/Configuration/_ShiftDefinitionsSection", model);
        }

        public async Task<IActionResult> StaffingRequirementsSection(int venueId, int? departmentId)
        {
            var model = await BuildDashboardModel(venueId, departmentId);
            return PartialView("Partials/Configuration/_StaffingRequirementsSection", model);
        }

        public async Task<IActionResult> UsersSection(int? venueId, int? departmentId)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            // Return list of users who are members of the current workspace
            var userIds = await _context.WorkspaceMember
                .Where(wm => wm.WorkspaceId == wsId.Value)
                .Select(wm => wm.UserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Cast<ApplicationUser>()
                .ToListAsync();

            return PartialView("Partials/UserManagement/_UsersSection", users);
        }

        public async Task<IActionResult> ScheduleSection(int venueId, int? departmentId)
        {
            var model = await BuildDashboardModel(venueId, departmentId);
            return PartialView("Partials/ScheduleManagement/_ScheduleSection", model);
        }

        // ---------------------------
        // SHARED MODEL BUILDER
        // ---------------------------
        private async Task<DashboardViewModel> BuildDashboardModel(int venueId, int? departmentId = null)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue)
            {
                return new DashboardViewModel();
            }

            var venues = await _context.Venue.ToListAsync();
            var selectedVenue = venues.FirstOrDefault(v => v.Id == venueId);

            var rolesQuery = _context.Role.AsQueryable(); // :)

            // Ensure roles are restricted to the workspace (global filters apply, but defend in depth)
            rolesQuery = rolesQuery.Where(r => r.WorkspaceId == wsId.Value);

            if (departmentId.HasValue)
            {
                rolesQuery = rolesQuery
                    .Where(r => r.DepartmentId == departmentId);
            }

            var roles = await rolesQuery.ToListAsync();
            var departments = await _context.Department.Where(d => d.VenueId == venueId).ToListAsync();
            var departmentIds = departments.Select(d => d.Id).ToList();

            var shiftDefinitionsQuery = _context.ShiftDefinition
                .Include(s => s.Department)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                shiftDefinitionsQuery = shiftDefinitionsQuery
                    .Where(s => s.DepartmentId == departmentId);
            }
            else
            {
                shiftDefinitionsQuery = shiftDefinitionsQuery
                    .Where(s => departmentIds.Contains(s.DepartmentId));
            }

            var shiftDefinitions = await shiftDefinitionsQuery.ToListAsync();

            var staffingRequirementsQuery = _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Include(r => r.Department)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                staffingRequirementsQuery = staffingRequirementsQuery
                    .Where(r => r.DepartmentId == departmentId);
            }
            else
            {
                staffingRequirementsQuery = staffingRequirementsQuery
                    .Where(r => departmentIds.Contains(r.DepartmentId));
            }

            var staffingRequirements = await staffingRequirementsQuery.ToListAsync();

            var today = DateTime.Today;
            var maxDate = today.AddMonths(6);

            var scheduledShiftsQuery = _context.ScheduledShift
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Where(s => s.VenueId == venueId &&
                            s.Date >= today &&
                            s.Date <= maxDate);


            if (departmentId.HasValue)
            {
                scheduledShiftsQuery = scheduledShiftsQuery
                    .Where(s => s.DepartmentId == departmentId);
            }

            var scheduledShifts = await scheduledShiftsQuery
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
            public Department? SelectedDepartment { get; set; }
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
