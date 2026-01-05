using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class SeedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "Aktivtkonto", "Beskrivning", "BildUrl", "Efternamn", "Email", "Fornamn", "Losenord", "Telefonnummer", "Yrkestitel" },
                values: new object[] { 1, true, "Detta är en testprofil skapad via kod.", "", "Test", "test@test.se", "Oscar", "123", 701234567, "Systemutvecklare" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
