using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Turnus.Models;
using Turnus.Services.Authorization;
using Xunit;

namespace TurnusTests
{
    class FakeWorkspaceProvider : Turnus.Services.ICurrentWorkspaceProvider
    {
        private readonly WorkspaceMember? _member;
        private readonly int? _wsId;
        public FakeWorkspaceProvider(int? wsId, WorkspaceMember? member)
        {
            _wsId = wsId;
            _member = member;
        }
        public System.Threading.Tasks.Task<int?> GetWorkspaceIdAsync() => System.Threading.Tasks.Task.FromResult(_wsId);
        public System.Threading.Tasks.Task<WorkspaceMember?> GetCurrentMemberAsync() => System.Threading.Tasks.Task.FromResult(_member);
    }

    public class WorkspaceHandlersTests
    {
        [Fact]
        public async System.Threading.Tasks.Task WorkspaceMemberHandler_Allows_SuperAdmin()
        {
            var requirement = new WorkspaceMemberRequirement();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "SuperAdmin") }));
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

            var handler = new WorkspaceMemberHandler(new FakeWorkspaceProvider(null, null));
            await handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async System.Threading.Tasks.Task WorkspaceManagerHandler_Allows_Manager_Member()
        {
            var requirement = new WorkspaceManagerRequirement();
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var member = new WorkspaceMember { WorkspaceId = 1, UserId = "u", Role = WorkspaceRole.Manager };
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

            var handler = new WorkspaceManagerHandler(new FakeWorkspaceProvider(1, member));
            await handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async System.Threading.Tasks.Task WorkspaceManagerHandler_Denies_NonMember()
        {
            var requirement = new WorkspaceManagerRequirement();
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

            var handler = new WorkspaceManagerHandler(new FakeWorkspaceProvider(1, null));
            await handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }
    }
}
