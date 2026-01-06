using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c326557f-10cc-425d-bd82-5160d6ebe9c9", "AQAAAAIAAYagAAAAEFt9hDcB8ikP7LMEqgXaJwWN/OftWIk7c4XJrQG2xb1vK2039Eaw1m7dEq/CD5+msQ==", "62214848-1922-4eef-97fc-9c2937e44c55" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4d7dfc89-ec98-454d-b145-5c76c5a49789", "AQAAAAIAAYagAAAAEOUxY+9VOr+PhkOQi6WustdeglJXmj1yziEETYMxFY2pX0Kgjrk5Yu8iSDXHTeVWLQ==", "7d16287c-a6d0-4f79-9a23-de085d2654ab" });
        }
    }
}
