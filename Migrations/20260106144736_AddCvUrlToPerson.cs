using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class AddCvUrlToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fil",
                table: "Projekt");

            migrationBuilder.AlterColumn<string>(
                name: "Yrkestitel",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Telefonnummer",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "BildUrl",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Beskrivning",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tidsstampel",
                table: "Meddelanden",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<string>(
                name: "AvsandareNamn",
                table: "Meddelanden",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MottagareId",
                table: "Meddelanden",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "83570413-ba90-4bb7-b7d1-becf6055f196", "AQAAAAIAAYagAAAAEL71XgZ8oGqpuUg4UDbHXhWn5O9qTW6IOaA/Jc7y701RxMovJnarTytXDQAmtVYNPw==", "a60cf775-00f1-4804-8459-0dd14f102332" });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CvUrl", "Telefonnummer" },
                values: new object[] { null, "0701234567" });

            migrationBuilder.CreateIndex(
                name: "IX_Meddelanden_MottagareId",
                table: "Meddelanden",
                column: "MottagareId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meddelanden_Persons_MottagareId",
                table: "Meddelanden",
                column: "MottagareId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meddelanden_Persons_MottagareId",
                table: "Meddelanden");

            migrationBuilder.DropIndex(
                name: "IX_Meddelanden_MottagareId",
                table: "Meddelanden");

            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "AvsandareNamn",
                table: "Meddelanden");

            migrationBuilder.DropColumn(
                name: "MottagareId",
                table: "Meddelanden");

            migrationBuilder.AddColumn<string>(
                name: "Fil",
                table: "Projekt",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Yrkestitel",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Telefonnummer",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BildUrl",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Beskrivning",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Tidsstampel",
                table: "Meddelanden",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c326557f-10cc-425d-bd82-5160d6ebe9c9", "AQAAAAIAAYagAAAAEFt9hDcB8ikP7LMEqgXaJwWN/OftWIk7c4XJrQG2xb1vK2039Eaw1m7dEq/CD5+msQ==", "62214848-1922-4eef-97fc-9c2937e44c55" });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 1,
                column: "Telefonnummer",
                value: 701234567);
        }
    }
}
