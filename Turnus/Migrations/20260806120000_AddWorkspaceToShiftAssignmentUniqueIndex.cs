using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnus.Migrations
{
    public partial class AddWorkspaceToShiftAssignmentUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old unique index
            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignment_ScheduledShiftId_EmployeeId_RoleId",
                table: "ShiftAssignment");

            // Create a new unique index that includes WorkspaceId to scope uniqueness per workspace
            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignment_ScheduledShiftId_EmployeeId_RoleId_WorkspaceId",
                table: "ShiftAssignment",
                columns: new[] { "ScheduledShiftId", "EmployeeId", "RoleId", "WorkspaceId" },
                unique: true,
                filter: "[WorkspaceId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignment_ScheduledShiftId_EmployeeId_RoleId_WorkspaceId",
                table: "ShiftAssignment");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignment_ScheduledShiftId_EmployeeId_RoleId",
                table: "ShiftAssignment",
                columns: new[] { "ScheduledShiftId", "EmployeeId", "RoleId" },
                unique: true);
        }
    }
}
