using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Persons",
                newName: "Yrkestitel");

            migrationBuilder.AddColumn<bool>(
                name: "Aktivtkonto",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Beskrivning",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BildUrl",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Efternamn",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fornamn",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Losenord",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Telefonnummer",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Innehall = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tidsstampel = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ArLast = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Projektnamn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beskrivning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Startdatum = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Slutdatum = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropColumn(
                name: "Aktivtkonto",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Beskrivning",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "BildUrl",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Efternamn",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Fornamn",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Losenord",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Telefonnummer",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "Yrkestitel",
                table: "Persons",
                newName: "Name");
        }
    }
}
