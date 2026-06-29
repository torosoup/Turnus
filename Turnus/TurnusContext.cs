using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class TurnusContext(DbContextOptions<TurnusContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Role> Role { get; set; } = default!;
    public DbSet<Venue> Venue { get; set; } = default!;
    public DbSet<VenueStaffingRequirement> VenueStaffingRequirement { get; set; } = default!;
    public DbSet<ShiftDefinition> ShiftDefinition { get; set; } = default!;
    public DbSet<ScheduledDay> ScheduledDay { get; set; } = default!;
    public DbSet<ScheduledShift> ScheduledShift { get; set; } = default!;
    public DbSet<Availability> Availability { get; set; } = default!;
    public DbSet<ShiftAssignment> ShiftAssignment { get; set; } = default!;
    public DbSet<DayAssignment> DayAssignment { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ScheduledShift>()
            .HasOne(s => s.ShiftDefinition)
            .WithMany()
            .HasForeignKey(s => s.ShiftDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}