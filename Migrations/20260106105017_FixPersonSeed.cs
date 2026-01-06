using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class FixPersonSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5fd5baa1-318c-425f-bd1e-adf7f9d1b66b", "AQAAAAIAAYagAAAAEKI7xmOvLGH2mYVgwH/R/WxoWV4NeLJnatD/Nmo7J/YxTRFM6L0z7FXgoqS45URruA==", "04f2c671-3f29-4ea5-ae83-df9a0963a886" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "381d47db-9b30-4b11-ac27-0620c105f749", "AQAAAAIAAYagAAAAEPkckbddLBBI+Xd8YJaxA0s6Rjlol1Ohf02FGKXMZmiSp+Ek36jtCNCAwl/2CO6PRg==", "2fb71647-5991-4a3e-a62b-c87f6176bfef" });
        }
    }
}
