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
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public HomeController(TurnusContext context, UserManager<ApplicationUser> userManager, Microsoft.Extensions.Configuration.IConfiguration configuration, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _workspaceProvider = workspaceProvider;
        }

        public async Task<IActionResult> Index(string? actionAfterLogin, string? workspaceName)
        {
            var today = DateTime.Today;

            // Upcoming window configurable via appsettings: Home:UpcomingDays (default 30)
            var upcomingDays = _configuration.GetValue<int?>("Home:UpcomingDays") ?? 30;

            // Upcoming general shifts (next upcomingDays)
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();

            // If no workspace is active, we treat the request as landing page / onboarding.
            if (!wsId.HasValue)
            {
                // If an actionAfterLogin was requested and the user is not authenticated, redirect to login
                if (!string.IsNullOrEmpty(actionAfterLogin) && !User.Identity?.IsAuthenticated == true)
                {
                    var returnUrl = Url.Action("Index", "Home", new { actionAfterLogin = actionAfterLogin, workspaceName = workspaceName });
                    return Redirect($"/Identity/Account/Login?returnUrl={System.Net.WebUtility.UrlEncode(returnUrl)}");
                }

                // If the user is authenticated and requested an action, perform it
                if (!string.IsNullOrEmpty(actionAfterLogin) && User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User)!;

                    if (actionAfterLogin == "create" && !string.IsNullOrEmpty(workspaceName))
                    {
                        // Trim and validate workspace name
                        var name = workspaceName.Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            ModelState.AddModelError("", "Workspace name is required.");
                            return View("Landing");
                        }
                        if (name.Length < 2 || name.Length > 100)
                        {
                            ModelState.AddModelError("", "Workspace name must be between 2 and 100 characters.");
                            return View("Landing");
                        }
                        var allowed = System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9 _-]+$");
                        if (!allowed)
                        {
                            ModelState.AddModelError("", "Workspace name contains invalid characters.");
                            return View("Landing");
                        }

                        // Check if a workspace with this name exists
                        var exists = await _context.Workspace.FirstOrDefaultAsync(w => w.Name == name);
                        if (exists != null)
                        {
                            ModelState.AddModelError("", "Workspace name already exists.");
                            return View("Landing");
                        }

                        var ws = new Workspace { Name = name, CreatedAt = DateTime.UtcNow, CreatedByUserId = userId };
                        _context.Workspace.Add(ws);
                        await _context.SaveChangesAsync();

                        // Add membership as Owner
                        _context.WorkspaceMember.Add(new WorkspaceMember { WorkspaceId = ws.Id, UserId = userId, Role = WorkspaceRole.Owner, JoinedAt = DateTime.UtcNow });
                        await _context.SaveChangesAsync();

                        // Set workspace cookie so provider and middleware pick it up
                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax };
                        if (Request.IsHttps) cookieOptions.Secure = true;
                        Response.Cookies.Append("workspace", ws.Id.ToString(), cookieOptions);

                        return RedirectToAction("Dashboard", "Admin");
                    }

                    if (actionAfterLogin == "join" && !string.IsNullOrEmpty(workspaceName))
                    {
                        var name = workspaceName.Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            ModelState.AddModelError("", "Workspace name is required.");
                            return View("Landing");
                        }

                        var ws = await _context.Workspace.FirstOrDefaultAsync(w => w.Name == name);
                        if (ws == null)
                        {
                            ModelState.AddModelError("", "Workspace not found.");
                            return View("Landing");
                        }

                        // Add membership as Member if not exists
                        var member = await _context.WorkspaceMember.FindAsync(ws.Id, _userManager.GetUserId(User));
                        if (member == null)
                        {
                            _context.WorkspaceMember.Add(new WorkspaceMember { WorkspaceId = ws.Id, UserId = userId, Role = WorkspaceRole.Member, JoinedAt = DateTime.UtcNow });
                            await _context.SaveChangesAsync();
                        }

                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax };
                        if (Request.IsHttps) cookieOptions.Secure = true;
                        Response.Cookies.Append("workspace", ws.Id.ToString(), cookieOptions);

                        return RedirectToAction("Index", "Schedule");
                    }

                    // Unknown action, show landing
                    return View("Landing");
                }

                // No active workspace and no action => show landing
                return View("Landing");
            }

            var upcomingQuery = _context.ScheduledShift
                .Include(s => s.Venue)
                .Include(s => s.Department)
                .Include(s => s.ShiftDefinition)
                .Where(s => s.Date.Date >= today && s.Date.Date <= today.AddDays(upcomingDays));

            if (wsId.HasValue)
            {
                upcomingQuery = upcomingQuery.Where(s => s.WorkspaceId == wsId.Value);
            }

            var upcomingShifts = await upcomingQuery
                .OrderBy(s => s.Date)
                .Take(10)
                .ToListAsync();

            List<ShiftAssignment> myUpcoming = new();
            int completedCount = 0;

            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User)!;

                var assignmentQuery = _context.ShiftAssignment
                    .Include(a => a.ScheduledShift!)
                        .ThenInclude(s => s.Venue)
                    .Include(a => a.ScheduledShift!)
                        .ThenInclude(s => s.Department)
                    .Include(a => a.ScheduledShift!)
                        .ThenInclude(s => s.ShiftDefinition)
                    .Include(a => a.Role)
                    .Where(a => a.EmployeeId == userId && a.ScheduledShift != null && a.ScheduledShift.Date.Date >= today);

                if (wsId.HasValue)
                {
                    assignmentQuery = assignmentQuery.Where(a => a.WorkspaceId == wsId.Value);
                }

                myUpcoming = await assignmentQuery
                    .OrderBy(a => a.ScheduledShift!.Date)
                    .Take(10)
                    .ToListAsync();

                var completedQuery = _context.ShiftAssignment
                    .Include(a => a.ScheduledShift!)
                    .Where(a => a.EmployeeId == userId && a.ScheduledShift != null && a.ScheduledShift.Date.Date < today);

                if (wsId.HasValue)
                {
                    completedQuery = completedQuery.Where(a => a.WorkspaceId == wsId.Value);
                }

                completedCount = await completedQuery.CountAsync();
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
