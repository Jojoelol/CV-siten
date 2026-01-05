using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_siten.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyPersonProjekt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonProjekt",
                columns: table => new
                {
                    PersonerId = table.Column<int>(type: "int", nullable: false),
                    ProjektId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonProjekt", x => new { x.PersonerId, x.ProjektId });
                    table.ForeignKey(
                        name: "FK_PersonProjekt_Persons_PersonerId",
                        column: x => x.PersonerId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonProjekt_Projekt_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonProjekt_ProjektId",
                table: "PersonProjekt",
                column: "ProjektId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonProjekt");
        }
    }
}
