using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class TurnusContext(DbContextOptions<TurnusContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Role> Role { get; set; } = default!;
    public DbSet<Venue> Venue { get; set; } = default!;
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
    }


}