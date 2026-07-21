using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Turnus.Models;
using Turnus.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace Turnus.Tests
{
    public class ShiftAssignmentTests
    {
        private TurnusContext CreateSqliteContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<TurnusContext>()
                .UseSqlite(connection)
                .Options;

            return new TurnusContext(options);
        }

        [Fact]
        public async Task AssignShift_Enforces_ShiftScoped_Requirement()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            using var context = CreateSqliteContext(connection);
            context.Database.EnsureCreated();

            // seed venue -> department -> shift definition -> role -> requirement -> scheduled shift -> users
            var venue = new Venue { Name = "V" };
            context.Venue.Add(venue);
            await context.SaveChangesAsync();

            var dept = new Department { Name = "D", VenueId = venue.Id };
            context.Department.Add(dept);
            await context.SaveChangesAsync();

            var def = new ShiftDefinition { Name = "S", DepartmentId = dept.Id, StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(1) };
            context.ShiftDefinition.Add(def);
            await context.SaveChangesAsync();

            var role = new Role { Name = "R", DepartmentId = dept.Id };
            context.Role.Add(role);
            await context.SaveChangesAsync();

            var requirement = new VenueStaffingRequirement { DepartmentId = dept.Id, RoleId = role.Id, RequiredCount = 1, IsShiftScoped = true };
            context.VenueStaffingRequirement.Add(requirement);
            await context.SaveChangesAsync();

            var scheduled = new ScheduledShift { VenueId = venue.Id, DepartmentId = dept.Id, ShiftDefinitionId = def.Id, Date = DateTime.Today };
            context.ScheduledShift.Add(scheduled);
            await context.SaveChangesAsync();

            var user1 = new ApplicationUser { UserName = "u1@example.com", Email = "u1@example.com" };
            var user2 = new ApplicationUser { UserName = "u2@example.com", Email = "u2@example.com" };
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var controller = new ShiftAssignmentController(context);

            var model1 = new ShiftAssignment { ScheduledShiftId = scheduled.Id, RoleId = role.Id, EmployeeId = user1.Id };
            var result1 = await controller.AssignShift(model1, venue.Id, scheduled.Date);
            Assert.IsType<RedirectToActionResult>(result1);

            var model2 = new ShiftAssignment { ScheduledShiftId = scheduled.Id, RoleId = role.Id, EmployeeId = user2.Id };
            var result2 = await controller.AssignShift(model2, venue.Id, scheduled.Date);
            Assert.IsType<BadRequestObjectResult>(result2);
        }

        [Fact]
        public async Task ShiftAssignment_UniqueIndex_Prevents_Duplicate_Save()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            using var context = CreateSqliteContext(connection);
            context.Database.EnsureCreated();

            var venue = new Venue { Name = "V" };
            context.Venue.Add(venue);
            await context.SaveChangesAsync();

            var dept = new Department { Name = "D", VenueId = venue.Id };
            context.Department.Add(dept);
            await context.SaveChangesAsync();

            var def = new ShiftDefinition { Name = "Def", DepartmentId = dept.Id, StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromHours(1) };
            context.ShiftDefinition.Add(def);
            await context.SaveChangesAsync();

            var role = new Role { Name = "R", DepartmentId = dept.Id };
            context.Role.Add(role);
            await context.SaveChangesAsync();

            var scheduled = new ScheduledShift { VenueId = venue.Id, DepartmentId = dept.Id, ShiftDefinitionId = def.Id, Date = DateTime.Today };
            context.ScheduledShift.Add(scheduled);
            await context.SaveChangesAsync();

            var user = new ApplicationUser { UserName = "u@example.com", Email = "u@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var a1 = new ShiftAssignment { ScheduledShiftId = scheduled.Id, EmployeeId = user.Id, RoleId = role.Id };
            context.ShiftAssignment.Add(a1);
            await context.SaveChangesAsync();

            var a2 = new ShiftAssignment { ScheduledShiftId = scheduled.Id, EmployeeId = user.Id, RoleId = role.Id };
            context.ShiftAssignment.Add(a2);

            await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => context.SaveChangesAsync());
        }
    }
}
