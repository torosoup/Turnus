using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnus.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "VenueStaffingRequirement",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Venue",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "ShiftDefinition",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "ShiftAssignment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "ScheduledShift",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Role",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Department",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Availability",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workspace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceMember",
                columns: table => new
                {
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMember", x => new { x.WorkspaceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_WorkspaceMember_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspaceMember_Workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueStaffingRequirement_WorkspaceId",
                table: "VenueStaffingRequirement",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Venue_WorkspaceId",
                table: "Venue",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDefinition_WorkspaceId",
                table: "ShiftDefinition",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignment_WorkspaceId",
                table: "ShiftAssignment",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledShift_WorkspaceId",
                table: "ScheduledShift",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_WorkspaceId",
                table: "Role",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_WorkspaceId",
                table: "Department",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Availability_WorkspaceId",
                table: "Availability",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMember_UserId",
                table: "WorkspaceMember",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availability_Workspace_WorkspaceId",
                table: "Availability",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Workspace_WorkspaceId",
                table: "Department",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Workspace_WorkspaceId",
                table: "Role",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledShift_Workspace_WorkspaceId",
                table: "ScheduledShift",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftAssignment_Workspace_WorkspaceId",
                table: "ShiftAssignment",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftDefinition_Workspace_WorkspaceId",
                table: "ShiftDefinition",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Venue_Workspace_WorkspaceId",
                table: "Venue",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueStaffingRequirement_Workspace_WorkspaceId",
                table: "VenueStaffingRequirement",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availability_Workspace_WorkspaceId",
                table: "Availability");

            migrationBuilder.DropForeignKey(
                name: "FK_Department_Workspace_WorkspaceId",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_Workspace_WorkspaceId",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledShift_Workspace_WorkspaceId",
                table: "ScheduledShift");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftAssignment_Workspace_WorkspaceId",
                table: "ShiftAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftDefinition_Workspace_WorkspaceId",
                table: "ShiftDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_Venue_Workspace_WorkspaceId",
                table: "Venue");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueStaffingRequirement_Workspace_WorkspaceId",
                table: "VenueStaffingRequirement");

            migrationBuilder.DropTable(
                name: "WorkspaceMember");

            migrationBuilder.DropTable(
                name: "Workspace");

            migrationBuilder.DropIndex(
                name: "IX_VenueStaffingRequirement_WorkspaceId",
                table: "VenueStaffingRequirement");

            migrationBuilder.DropIndex(
                name: "IX_Venue_WorkspaceId",
                table: "Venue");

            migrationBuilder.DropIndex(
                name: "IX_ShiftDefinition_WorkspaceId",
                table: "ShiftDefinition");

            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignment_WorkspaceId",
                table: "ShiftAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledShift_WorkspaceId",
                table: "ScheduledShift");

            migrationBuilder.DropIndex(
                name: "IX_Role_WorkspaceId",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Department_WorkspaceId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Availability_WorkspaceId",
                table: "Availability");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "VenueStaffingRequirement");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "ShiftDefinition");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "ShiftAssignment");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "ScheduledShift");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Availability");
        }
    }
}
