using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class CreateMeddelandenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fil",
                table: "Projekt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tidsstampel",
                table: "Meddelanden",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<int>(
                name: "AvsandareId",
                table: "Meddelanden",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                values: new object[] { "aeee2718-5ec0-478a-9a26-362ef8ad1688", "AQAAAAIAAYagAAAAEMvsvgXB1n5Z7eHDODZ0f7bzXVFsbIxDj5GjQ4P2tkWS8r6j6+H/wTa/8Z7qJQzCrg==", "b5deac2c-51ff-4d8a-948a-0e33c92be10c" });

            migrationBuilder.CreateIndex(
                name: "IX_Meddelanden_AvsandareId",
                table: "Meddelanden",
                column: "AvsandareId");

            migrationBuilder.CreateIndex(
                name: "IX_Meddelanden_MottagareId",
                table: "Meddelanden",
                column: "MottagareId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meddelanden_Persons_AvsandareId",
                table: "Meddelanden",
                column: "AvsandareId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meddelanden_Persons_MottagareId",
                table: "Meddelanden",
                column: "MottagareId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meddelanden_Persons_AvsandareId",
                table: "Meddelanden");

            migrationBuilder.DropForeignKey(
                name: "FK_Meddelanden_Persons_MottagareId",
                table: "Meddelanden");

            migrationBuilder.DropIndex(
                name: "IX_Meddelanden_AvsandareId",
                table: "Meddelanden");

            migrationBuilder.DropIndex(
                name: "IX_Meddelanden_MottagareId",
                table: "Meddelanden");

            migrationBuilder.DropColumn(
                name: "AvsandareId",
                table: "Meddelanden");

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
        }
    }
}
