using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Turnus.Models;
using Turnus.Services;
using Xunit;

namespace TurnusTests
{
    public class CurrentWorkspaceProviderTests
    {
        private TurnusContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TurnusContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TurnusContext(options);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetWorkspaceFromClaimAsync()
        {
            var ctx = CreateContext("ws_claim");
            // seed workspace and member
            var ws = new Workspace { Name = "T1" };
            ctx.Workspace.Add(ws);
            await ctx.SaveChangesAsync();

            var user = new ApplicationUser { Id = "u1", Email = "a@b.com" };
            ctx.Users.Add(user);
            ctx.WorkspaceMember.Add(new WorkspaceMember { WorkspaceId = ws.Id, UserId = user.Id, Role = WorkspaceRole.Owner });
            await ctx.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim("workspace", ws.Id.ToString()) };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

            var accessor = new HttpContextAccessor { HttpContext = httpContext };
            var provider = new CurrentWorkspaceProvider(accessor, ctx);

            var resolved = await provider.GetWorkspaceIdAsync();
            Assert.Equal(ws.Id, resolved);

            var member = await provider.GetCurrentMemberAsync();
            Assert.NotNull(member);
            Assert.Equal(user.Id, member!.UserId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetWorkspaceFallbackAsync()
        {
            var ctx = CreateContext("ws_fallback");
            var ws = new Workspace { Name = "T2" };
            ctx.Workspace.Add(ws);
            var user = new ApplicationUser { Id = "u2", Email = "c@d.com" };
            ctx.Users.Add(user);
            ctx.WorkspaceMember.Add(new WorkspaceMember { WorkspaceId = ws.Id, UserId = user.Id, Role = WorkspaceRole.Member });
            await ctx.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

            var accessor = new HttpContextAccessor { HttpContext = httpContext };
            var provider = new CurrentWorkspaceProvider(accessor, ctx);

            var resolved = await provider.GetWorkspaceIdAsync();
            Assert.Equal(ws.Id, resolved);
        }
    }
}
