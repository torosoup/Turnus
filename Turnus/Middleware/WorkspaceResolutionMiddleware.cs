using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Turnus.Services;

namespace Turnus.Middleware
{
    public class WorkspaceResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public WorkspaceResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentWorkspaceProvider workspaceProvider, TurnusContext db)
        {
            // Resolve workspace for the current user and set it on the DbContext so global query filters apply.
            var wsId = await workspaceProvider.GetWorkspaceIdAsync();
            if (wsId.HasValue)
            {
                db.CurrentWorkspaceId = wsId.Value;
            }

            await _next(context);
        }
    }
}
