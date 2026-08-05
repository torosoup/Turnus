using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Turnus.Models;

namespace Turnus.Controllers
{
    [Authorize(Policy = "WorkspaceManager")]
    public class UsersController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Turnus.Services.ICurrentWorkspaceProvider _workspaceProvider;

        public UsersController(TurnusContext context, UserManager<ApplicationUser> userManager, Turnus.Services.ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _userManager = userManager;
            _workspaceProvider = workspaceProvider;
        }

        // GET: Users/Manage?id={id}
        public async Task<IActionResult> Manage(string id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Ensure the target user is a member of this workspace
            var member = await _context.WorkspaceMember.FindAsync(wsId.Value, user.Id);
            if (member == null) return NotFound();

            ViewBag.Roles = await _context.Role.Where(r => r.WorkspaceId == wsId.Value).ToListAsync();
            ViewBag.UserRoles = (await _userManager.GetRolesAsync(user)).ToList();

            return PartialView("~/Views/Admin/Partials/UserManagement/_ManageUser.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(string id, string[] selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent managers from modifying their own roles to avoid privilege changes via UI
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                return Forbid();
            }

            // Prevent role changes to other manager accounts (require higher privilege)
            var isTargetManager = await _userManager.IsInRoleAsync(user, "Manager");
            if (isTargetManager)
            {
                return Forbid();
            }

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
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Ensure the target user is a member of this workspace
            var member = await _context.WorkspaceMember.FindAsync(wsId.Value, user.Id);
            if (member == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            var assignments = await _context.ShiftAssignment
                .Include(a => a.ScheduledShift).ThenInclude(s => s.ShiftDefinition)
                .Where(a => a.EmployeeId == id && a.WorkspaceId == wsId.Value)
                .OrderByDescending(a => a.ScheduledShift.Date)
                .ToListAsync();

            ViewBag.Assignments = assignments;

            return PartialView("~/Views/Admin/Partials/UserManagement/_UserDetails.cshtml", user);
        }

        // GET: Users/Delete?id={id}
        public async Task<IActionResult> Delete(string id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var member = await _context.WorkspaceMember.FindAsync(wsId.Value, user.Id);
            if (member == null) return NotFound();

            return PartialView("~/Views/Admin/Partials/UserManagement/_DeleteUser.cshtml", user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue) return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var member = await _context.WorkspaceMember.FindAsync(wsId.Value, user.Id);
                if (member == null) return NotFound();

                var currentUserId = _userManager.GetUserId(User);

                // Prevent deleting yourself
                if (currentUserId == user.Id) return Forbid();

                // Prevent deleting other owner accounts via this UI
                if (member.Role == WorkspaceRole.Owner) return Forbid();

                // Remove workspace membership and optionally delete user if desired
                _context.WorkspaceMember.Remove(member);

                // If you want to delete the user globally, ensure only SuperAdmin can do that.
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
