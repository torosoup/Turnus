using Microsoft.EntityFrameworkCore;

public class TurnusContext(DbContextOptions<TurnusContext> options) : DbContext(options)
{
    public DbSet<Turnus.Models.EventTemplate> EventTemplate { get; set; } = default!;
    public DbSet<Turnus.Models.Role> Role { get; set; } = default!;
    public DbSet<Turnus.Models.EventTemplateRole> EventTemplateRole { get; set; } = default!;
    public DbSet<Turnus.Models.ScheduledEvent> ScheduledEvent { get; set; } = default!;
}
