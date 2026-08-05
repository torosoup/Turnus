using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;
using Turnus.Services;

namespace Turnus.Controllers
{
    [Authorize]
    public class DiagnosticsController : Controller
    {
        private readonly TurnusContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentWorkspaceProvider _workspaceProvider;

        public DiagnosticsController(TurnusContext context, UserManager<ApplicationUser> userManager, ICurrentWorkspaceProvider workspaceProvider)
        {
            _context = context;
            _userManager = userManager;
            _workspaceProvider = workspaceProvider;
        }

        // GET: /Diagnostics/Workspace
        public async Task<IActionResult> Workspace()
        {
            var userId = _userManager.GetUserId(User);
            var email = User.Identity?.Name ?? await _userManager.GetEmailAsync(await _userManager.GetUserAsync(User));
            var roles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

            var resolvedWorkspaceId = await _workspaceProvider.GetWorkspaceIdAsync();
            var currentMember = await _workspaceProvider.GetCurrentMemberAsync();

            var memberships = await _context.WorkspaceMember
                .Where(wm => wm.UserId == userId)
                .Include(wm => wm.Workspace)
                .Select(wm => new { wm.WorkspaceId, WorkspaceName = wm.Workspace!.Name, wm.Role })
                .ToListAsync();

            return Json(new
            {
                UserId = userId,
                Email = email,
                IdentityRoles = roles,
                ResolvedWorkspaceId = resolvedWorkspaceId,
                CurrentMember = currentMember == null ? null : new { currentMember.WorkspaceId, currentMember.UserId, currentMember.Role, currentMember.JoinedAt },
                Memberships = memberships
            });
        }
    }
}
