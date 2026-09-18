using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddBulletinDecisions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulletinDecisions",
                columns: table => new
                {
                    IdBulletinDecision = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    IdPeriode = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppreciationGenerale = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdAuteur = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletinDecisions", x => x.IdBulletinDecision);
                    table.ForeignKey(
                        name: "FK_BulletinDecisions_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletinDecisions_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletinDecisions_PeriodesCotation_IdPeriode",
                        column: x => x.IdPeriode,
                        principalTable: "PeriodesCotation",
                        principalColumn: "IdPeriode",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinDecisions_Eleve_Annee_Periode",
                table: "BulletinDecisions",
                columns: new[] { "IdEleve", "IdAnneeScolaire", "IdPeriode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BulletinDecisions_IdAnneeScolaire",
                table: "BulletinDecisions",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinDecisions_IdPeriode",
                table: "BulletinDecisions",
                column: "IdPeriode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BulletinDecisions");
        }
    }
}
