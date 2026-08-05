using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Turnus.Models;

namespace TurnusTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace DB with InMemory
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TurnusContext>));
                services.Remove(descriptor);

                services.AddDbContext<TurnusContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });

                // Build service provider and seed data
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<TurnusContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // seed workspace and a membership for test user 'test-user'
                    var ws = new Workspace { Name = "TestWs" };
                    db.Workspace.Add(ws);
                    db.SaveChanges();

                    db.WorkspaceMember.Add(new WorkspaceMember { WorkspaceId = ws.Id, UserId = "test-user", Role = WorkspaceRole.Owner });

                    // seed a scheduled shift in future
                    var venue = new Venue { Name = "V1", WorkspaceId = ws.Id };
                    db.Venue.Add(venue);
                    db.SaveChanges();

                    var dept = new Department { Name = "D1", VenueId = venue.Id, WorkspaceId = ws.Id };
                    db.Department.Add(dept);
                    db.SaveChanges();

                    var sdef = new ShiftDefinition { Name = "S1", DepartmentId = dept.Id, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), WorkspaceId = ws.Id };
                    db.ShiftDefinition.Add(sdef);
                    db.SaveChanges();

                    var sched = new ScheduledShift { VenueId = venue.Id, DepartmentId = dept.Id, ShiftDefinitionId = sdef.Id, Date = DateTime.Today.AddDays(1), WorkspaceId = ws.Id };
                    db.ScheduledShift.Add(sched);
                    db.SaveChanges();
                }
            });

            builder.ConfigureTestServices(services => { });
        }
    }
}
