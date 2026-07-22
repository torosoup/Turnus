using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Turnus.Models;

namespace Turnus.Controllers
{
    public class UsersController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(TurnusContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Users/Manage?id={id}
        public async Task<IActionResult> Manage(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = await _context.Role.ToListAsync();
            ViewBag.UserRoles = (await _userManager.GetRolesAsync(user)).ToList();

            return PartialView("~/Views/Admin/Partials/UserManagement/_ManageUser.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(string id, string[] selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (selectedRoles != null && selectedRoles.Length > 0)
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        // GET: Users/Details?id={id}
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            var assignments = await _context.ShiftAssignment
                .Include(a => a.ScheduledShift).ThenInclude(s => s.ShiftDefinition)
                .Where(a => a.EmployeeId == id)
                .OrderByDescending(a => a.ScheduledShift.Date)
                .ToListAsync();

            ViewBag.Assignments = assignments;

            return PartialView("~/Views/Admin/Partials/UserManagement/_UserDetails.cshtml", user);
        }

        // GET: Users/Delete?id={id}
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/UserManagement/_DeleteUser.cshtml", user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
