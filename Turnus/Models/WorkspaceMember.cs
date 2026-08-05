using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnus.Models
{
    public enum WorkspaceRole // enum to represent the role of a user in a workspace; 0 = Owner, 1 = Admin, 2 = Manager, 3 = Member
    {
        Owner,
        Admin,
        Manager,
        Member
    }

    public class WorkspaceMember
    {
        [Required]
        public int WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
