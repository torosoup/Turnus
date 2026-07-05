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

        public async Task<IActionResult> Dashboard(int? venueId)
        {
            var venues = await _context.Venue.ToListAsync();

            // Select first venue if none specified
            var selectedVenue = venueId.HasValue
                ? venues.FirstOrDefault(v => v.Id == venueId.Value)
                : venues.FirstOrDefault();

            if (selectedVenue == null)
            {
                return View(new DashboardViewModel { Venues = venues });
            }

            var roles = await _context.Role.ToListAsync();
            var shiftDefinitions = await _context.ShiftDefinition
                .Where(s => s.VenueId == selectedVenue.Id)
                .ToListAsync();
            var staffingRequirements = await _context.VenueStaffingRequirement
                .Include(r => r.Role)
                .Where(r => r.VenueId == selectedVenue.Id)
                .ToListAsync();
            var scheduledDays = await _context.ScheduledDay
                .Include(d => d.ScheduledShifts)
                    .ThenInclude(s => s.ShiftDefinition)
                .Where(d => d.VenueId == selectedVenue.Id)
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Completion checks
            bool hasRoles = roles.Any();
            bool hasShiftDefinitions = shiftDefinitions.Any();
            bool hasStaffingRequirements = staffingRequirements.Any();
            bool venueSettingsComplete = hasRoles && hasShiftDefinitions && hasStaffingRequirements;

            var viewModel = new DashboardViewModel
            {
                Venues = venues,
                SelectedVenue = selectedVenue,
                Roles = roles,
                ShiftDefinitions = shiftDefinitions,
                StaffingRequirements = staffingRequirements,
                ScheduledDays = scheduledDays,
                HasRoles = hasRoles,
                HasShiftDefinitions = hasShiftDefinitions,
                HasStaffingRequirements = hasStaffingRequirements,
                VenueSettingsComplete = venueSettingsComplete
            };

            return View(viewModel);
        }

        // Nested view model
        public class DashboardViewModel
        {
            public List<Venue> Venues { get; set; } = new();
            public Venue? SelectedVenue { get; set; }
            public List<Role> Roles { get; set; } = new();
            public List<ShiftDefinition> ShiftDefinitions { get; set; } = new();
            public List<VenueStaffingRequirement> StaffingRequirements { get; set; } = new();
            public List<ScheduledDay> ScheduledDays { get; set; } = new();
            public bool HasRoles { get; set; }
            public bool HasShiftDefinitions { get; set; }
            public bool HasStaffingRequirements { get; set; }
            public bool VenueSettingsComplete { get; set; }
        }
    }
}