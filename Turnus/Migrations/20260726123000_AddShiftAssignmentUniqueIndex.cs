using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnus.Migrations
{
    public partial class AddShiftAssignmentUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignment_ScheduledShiftId_EmployeeId_RoleId",
                table: "ShiftAssignment",
                columns: new[] { "ScheduledShiftId", "EmployeeId", "RoleId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignment_ScheduledShiftId_EmployeeId_RoleId",
                table: "ShiftAssignment");
        }
    }
}
