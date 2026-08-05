using Microsoft.AspNetCore.Authorization;

namespace Turnus.Services.Authorization
{
    public class WorkspaceMemberRequirement : IAuthorizationRequirement { }

    public class WorkspaceManagerRequirement : IAuthorizationRequirement { }
}
