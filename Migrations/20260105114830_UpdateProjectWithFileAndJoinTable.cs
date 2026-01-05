using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectWithFileAndJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fil",
                table: "Projekt",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Projekt",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Fil", "Slutdatum", "Startdatum" },
                values: new object[] { "exempel.pdf", new DateTimeOffset(new DateTime(2026, 2, 5, 12, 48, 29, 860, DateTimeKind.Unspecified).AddTicks(4001), new TimeSpan(0, 1, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 5, 12, 48, 29, 860, DateTimeKind.Unspecified).AddTicks(3916), new TimeSpan(0, 1, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fil",
                table: "Projekt");

            migrationBuilder.UpdateData(
                table: "Projekt",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Slutdatum", "Startdatum" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 5, 12, 40, 29, 718, DateTimeKind.Unspecified).AddTicks(8547), new TimeSpan(0, 1, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 5, 12, 40, 29, 718, DateTimeKind.Unspecified).AddTicks(8470), new TimeSpan(0, 1, 0, 0, 0)) });
        }
    }
}
