using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Snapshots de bulletins figés (après validation).
    /// Dépend de PeriodesCotation et BulletinDecisions déjà présents.
    /// </summary>
    public partial class AddBulletinsFiges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulletinsFiges",
                columns: table => new
                {
                    IdBulletinFige = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    IdPeriode = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MoyenneGenerale = table.Column<double>(type: "double", nullable: true),
                    Rang = table.Column<int>(type: "int", nullable: true),
                    EffectifClasse = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppreciationGenerale = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdAuteurValidation = table.Column<int>(type: "int", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletinsFiges", x => x.IdBulletinFige);
                    table.ForeignKey(
                        name: "FK_BulletinsFiges_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletinsFiges_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletinsFiges_PeriodesCotation_IdPeriode",
                        column: x => x.IdPeriode,
                        principalTable: "PeriodesCotation",
                        principalColumn: "IdPeriode",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinsFiges_Eleve_Annee_Periode",
                table: "BulletinsFiges",
                columns: new[] { "IdEleve", "IdAnneeScolaire", "IdPeriode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BulletinsFiges_IdAnneeScolaire",
                table: "BulletinsFiges",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinsFiges_IdPeriode",
                table: "BulletinsFiges",
                column: "IdPeriode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulletinsFiges");
        }
    }
}
