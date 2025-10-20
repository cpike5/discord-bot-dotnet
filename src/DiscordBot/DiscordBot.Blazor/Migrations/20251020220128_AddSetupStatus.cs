using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.Blazor.Migrations
{
    /// <inheritdoc />
    public partial class AddSetupStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SetupStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsComplete = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AdminUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    SetupVersion = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetupStatus_AspNetUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "SetupStatus",
                columns: new[] { "Id", "AdminUserId", "CompletedAt", "SetupVersion" },
                values: new object[] { 1, null, null, "1.0" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupStatus_AdminUserId",
                table: "SetupStatus",
                column: "AdminUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetupStatus");
        }
    }
}
