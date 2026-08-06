using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Turnus.Models;

namespace Turnus.Services
{
    public class CurrentWorkspaceProvider : ICurrentWorkspaceProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TurnusContext _db;
        private int? _cachedWorkspaceId;
        private WorkspaceMember? _cachedMember;

        public CurrentWorkspaceProvider(IHttpContextAccessor httpContextAccessor, TurnusContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public async System.Threading.Tasks.Task<int?> GetWorkspaceIdAsync()
        {
            if (_cachedWorkspaceId.HasValue) return _cachedWorkspaceId;

            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null || ctx.User?.Identity == null || !ctx.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            // 1) Check for explicit workspace claim
            var claim = ctx.User.FindFirst("workspace");
            if (claim != null && int.TryParse(claim.Value, out var wsIdFromClaim))
            {
                var member = await _db.WorkspaceMember.FindAsync(wsIdFromClaim, userId);
                if (member != null)
                {
                    _cachedWorkspaceId = wsIdFromClaim;
                    _cachedMember = member;
                    return _cachedWorkspaceId;
                }
            }

            // 2) Check for workspace cookie
            var cookieVal = ctx.Request.Cookies["workspace"];
            if (!string.IsNullOrEmpty(cookieVal) && int.TryParse(cookieVal, out var wsIdFromCookie))
            {
                var member = await _db.WorkspaceMember.FindAsync(wsIdFromCookie, userId);
                if (member != null)
                {
                    _cachedWorkspaceId = wsIdFromCookie;
                    _cachedMember = member;
                    return _cachedWorkspaceId;
                }
            }

            // No implicit fallback - user must explicitly join or select a workspace
            return null;
        }

        public async System.Threading.Tasks.Task<WorkspaceMember?> GetCurrentMemberAsync()
        {
            if (_cachedMember != null) return _cachedMember;

            var wsId = await GetWorkspaceIdAsync();
            if (!wsId.HasValue) return null;

            var ctx = _httpContextAccessor.HttpContext;
            var userId = ctx?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var member = await _db.WorkspaceMember.FindAsync(wsId.Value, userId);
            _cachedMember = member;
            return _cachedMember;
        }
    }
}
