using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Turnus.Models;

namespace Turnus.Controllers
{
    public class HomeController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public HomeController(TurnusContext context, UserManager<ApplicationUser> userManager, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // Upcoming window configurable via appsettings: Home:UpcomingDays (default 30)
            var upcomingDays = _configuration.GetValue<int?>("Home:UpcomingDays") ?? 30;

            // Upcoming general shifts (next upcomingDays)
            var upcomingShifts = await _context.ScheduledShift
                .Include(s => s.Venue)
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Where(s => s.Date.Date >= today && s.Date.Date <= today.AddDays(upcomingDays))
                .OrderBy(s => s.Date)
                .Take(10)
                .ToListAsync();

            List<ShiftAssignment> myUpcoming = new();
            int completedCount = 0;

            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User)!;

                myUpcoming = await _context.ShiftAssignment
                    .Include(a => a.ScheduledShift!)
                        .ThenInclude(s => s.Venue)
                    .Include(a => a.ScheduledShift!)
                        .ThenInclude(s => s.Department)
                    .Include(a => a.ScheduledShift!)
                        .ThenInclude(s => s.ShiftDefinition)
                    .Include(a => a.Role)
                    .Where(a => a.EmployeeId == userId && a.ScheduledShift != null && a.ScheduledShift.Date.Date >= today)
                    .OrderBy(a => a.ScheduledShift!.Date)
                    .Take(10)
                    .ToListAsync();

                completedCount = await _context.ShiftAssignment
                    .Include(a => a.ScheduledShift!)
                    .Where(a => a.EmployeeId == userId && a.ScheduledShift != null && a.ScheduledShift.Date.Date < today)
                    .CountAsync();
            }

            var model = new HomeIndexViewModel
            {
                UpcomingShifts = upcomingShifts,
                MyUpcomingAssignments = myUpcoming,
                CompletedShiftsCount = completedCount
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Nested view model for the home page
        public class HomeIndexViewModel
        {
            public List<ScheduledShift> UpcomingShifts { get; set; } = new();
            public List<ShiftAssignment> MyUpcomingAssignments { get; set; } = new();
            public int CompletedShiftsCount { get; set; }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
