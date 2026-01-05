using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonProjektModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonProjekt_Persons_PersonerId",
                table: "PersonProjekt");

            migrationBuilder.RenameColumn(
                name: "PersonerId",
                table: "PersonProjekt",
                newName: "PersonId");

            migrationBuilder.AddColumn<string>(
                name: "Roll",
                table: "PersonProjekt",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Projekt",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Slutdatum", "Startdatum" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 5, 12, 40, 29, 718, DateTimeKind.Unspecified).AddTicks(8547), new TimeSpan(0, 1, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 5, 12, 40, 29, 718, DateTimeKind.Unspecified).AddTicks(8470), new TimeSpan(0, 1, 0, 0, 0)) });

            migrationBuilder.AddForeignKey(
                name: "FK_PersonProjekt_Persons_PersonId",
                table: "PersonProjekt",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonProjekt_Persons_PersonId",
                table: "PersonProjekt");

            migrationBuilder.DropColumn(
                name: "Roll",
                table: "PersonProjekt");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "PersonProjekt",
                newName: "PersonerId");

            migrationBuilder.UpdateData(
                table: "Projekt",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Slutdatum", "Startdatum" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 5, 12, 22, 53, 141, DateTimeKind.Unspecified).AddTicks(4377), new TimeSpan(0, 1, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 5, 12, 22, 53, 141, DateTimeKind.Unspecified).AddTicks(4280), new TimeSpan(0, 1, 0, 0, 0)) });

            migrationBuilder.AddForeignKey(
                name: "FK_PersonProjekt_Persons_PersonerId",
                table: "PersonProjekt",
                column: "PersonerId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
