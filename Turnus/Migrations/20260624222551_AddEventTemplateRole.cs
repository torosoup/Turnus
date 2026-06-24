using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnus.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTemplateRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTemplateRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTemplateId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RequiredCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplateRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTemplateRole_EventTemplate_EventTemplateId",
                        column: x => x.EventTemplateId,
                        principalTable: "EventTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTemplateRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateRole_EventTemplateId",
                table: "EventTemplateRole",
                column: "EventTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplateRole_RoleId",
                table: "EventTemplateRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTemplateRole");
        }
    }
}
