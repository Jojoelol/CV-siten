using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class SeedProjekt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projekt",
                columns: new[] { "Id", "Beskrivning", "Projektnamn", "Slutdatum", "Startdatum", "Status", "Typ" },
                values: new object[] { 1, "Ett system byggt i .NET 8 med SQL Server.", "Globalt CV-System", new DateTimeOffset(new DateTime(2026, 2, 5, 12, 22, 53, 141, DateTimeKind.Unspecified).AddTicks(4377), new TimeSpan(0, 1, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 5, 12, 22, 53, 141, DateTimeKind.Unspecified).AddTicks(4280), new TimeSpan(0, 1, 0, 0, 0)), "Pågående", "Webbutveckling" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projekt",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
