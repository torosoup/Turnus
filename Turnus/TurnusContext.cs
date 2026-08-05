using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class TurnusContext(DbContextOptions<TurnusContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // This property is set per-request by middleware to scope queries to the active workspace.
    public int? CurrentWorkspaceId { get; set; }
    public DbSet<Role> Role { get; set; } = default!;
    public DbSet<Venue> Venue { get; set; } = default!;
    public DbSet<Workspace> Workspace { get; set; } = default!;
    public DbSet<WorkspaceMember> WorkspaceMember { get; set; } = default!;
    public DbSet<VenueStaffingRequirement> VenueStaffingRequirement { get; set; } = default!;
    public DbSet<ShiftDefinition> ShiftDefinition { get; set; } = default!;
    public DbSet<ScheduledShift> ScheduledShift { get; set; } = default!;
    public DbSet<Availability> Availability { get; set; } = default!;
    public DbSet<ShiftAssignment> ShiftAssignment { get; set; } = default!;
    public DbSet<Department> Department { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Prevent cascade delete cycle on ScheduledShift
        modelBuilder.Entity<ScheduledShift>()
            .HasOne(s => s.ShiftDefinition)
            .WithMany()
            .HasForeignKey(s => s.ShiftDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint — no duplicate shifts (same venue + date + shift definition)
        modelBuilder.Entity<ScheduledShift>()
            .HasIndex(s => new { s.VenueId, s.Date, s.ShiftDefinitionId })
            .IsUnique();

        // Prevent cascade delete cycle on Role nullable FKs
        modelBuilder.Entity<Role>()
            .HasOne(r => r.Venue)
            .WithMany()
            .HasForeignKey(r => r.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Role>()
            .HasOne(r => r.Department)
            .WithMany(d => d.Roles)
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        // Unique constraint to prevent duplicate assignments: ScheduledShiftId + EmployeeId + RoleId
        modelBuilder.Entity<ShiftAssignment>()
            .HasIndex(a => new { a.ScheduledShiftId, a.EmployeeId, a.RoleId })
            .IsUnique();

        // Global query filters to enforce workspace isolation. These filter expressions
        // reference the instance property CurrentWorkspaceId which is set at request time.
        modelBuilder.Entity<Venue>().HasQueryFilter(v => !CurrentWorkspaceId.HasValue || v.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !CurrentWorkspaceId.HasValue || d.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<Role>().HasQueryFilter(r => !CurrentWorkspaceId.HasValue || r.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<ShiftDefinition>().HasQueryFilter(s => !CurrentWorkspaceId.HasValue || s.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<ScheduledShift>().HasQueryFilter(s => !CurrentWorkspaceId.HasValue || s.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<VenueStaffingRequirement>().HasQueryFilter(r => !CurrentWorkspaceId.HasValue || r.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<ShiftAssignment>().HasQueryFilter(a => !CurrentWorkspaceId.HasValue || a.WorkspaceId == CurrentWorkspaceId);
        modelBuilder.Entity<Availability>().HasQueryFilter(a => !CurrentWorkspaceId.HasValue || a.WorkspaceId == CurrentWorkspaceId);

        // Workspace relationships: prevent accidental cascade across workspaces
        modelBuilder.Entity<Venue>()
            .HasOne(v => v.Workspace)
            .WithMany(w => w.Venues)
            .HasForeignKey(v => v.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkspaceMember>()
            .HasKey(wm => new { wm.WorkspaceId, wm.UserId });

        modelBuilder.Entity<WorkspaceMember>()
            .HasOne(wm => wm.Workspace)
            .WithMany(w => w.Members)
            .HasForeignKey(wm => wm.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }


}