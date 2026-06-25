using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;

public class TurnusContext(DbContextOptions<TurnusContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Turnus.Models.EventTemplate> EventTemplate { get; set; } = default!;
    public DbSet<Turnus.Models.Role> Role { get; set; } = default!;
    public DbSet<Turnus.Models.EventTemplateRole> EventTemplateRole { get; set; } = default!;
    public DbSet<Turnus.Models.ScheduledEvent> ScheduledEvent { get; set; } = default!;
    public DbSet<Turnus.Models.Availability> Availability { get; set; } = default!;
}
