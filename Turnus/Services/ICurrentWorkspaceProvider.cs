using Turnus.Models;

namespace Turnus.Services
{
    public interface ICurrentWorkspaceProvider
    {
        // Returns the active workspace id for the current request, or null if none.
        System.Threading.Tasks.Task<int?> GetWorkspaceIdAsync();

        // Returns the WorkspaceMember record for the current user and workspace, or null.
        System.Threading.Tasks.Task<WorkspaceMember?> GetCurrentMemberAsync();
    }
}
