using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Turnus.Models;
using Turnus.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Turnus.Services.Authorization
{
    public class WorkspaceMemberHandler : AuthorizationHandler<WorkspaceMemberRequirement>
    {
        private readonly ICurrentWorkspaceProvider _workspaceProvider;

        public WorkspaceMemberHandler(ICurrentWorkspaceProvider workspaceProvider)
        {
            _workspaceProvider = workspaceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WorkspaceMemberRequirement requirement)
        {
            // Allow super admins (global)
            if (context.User?.IsInRole("SuperAdmin") == true)
            {
                context.Succeed(requirement);
                return;
            }

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue)
            {
                return;
            }

            var member = await _workspaceProvider.GetCurrentMemberAsync();
            if (member != null)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class WorkspaceManagerHandler : AuthorizationHandler<WorkspaceManagerRequirement>
    {
        private readonly ICurrentWorkspaceProvider _workspaceProvider;

        public WorkspaceManagerHandler(ICurrentWorkspaceProvider workspaceProvider)
        {
            _workspaceProvider = workspaceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WorkspaceManagerRequirement requirement)
        {
            // Allow super admins (global)
            if (context.User?.IsInRole("SuperAdmin") == true)
            {
                context.Succeed(requirement);
                return;
            }

            var wsId = await _workspaceProvider.GetWorkspaceIdAsync();
            if (!wsId.HasValue)
            {
                return;
            }

            var member = await _workspaceProvider.GetCurrentMemberAsync();
            if (member == null) return;

            // Roles: Owner, Admin, Manager, Member
            // Allow Owner, Admin or Manager to pass
            if (member.Role == WorkspaceRole.Owner || member.Role == WorkspaceRole.Admin || member.Role == WorkspaceRole.Manager)
            {
                context.Succeed(requirement);
            }
        }
    }
}
