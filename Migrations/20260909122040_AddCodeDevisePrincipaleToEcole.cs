using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddCodeDevisePrincipaleToEcole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Frais_Classes_IdClasse",
                table: "Frais");

            migrationBuilder.DropForeignKey(
                name: "FK_Frais_Directions_IdDirection",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_Frais_Direction_Annee_Libelle_Classe",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_Frais_IdClasse",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_Frais_IdDirection",
                table: "Frais");

            migrationBuilder.DropColumn(
                name: "IdClasse",
                table: "Frais");

            migrationBuilder.RenameColumn(
                name: "IdDirection",
                table: "Frais",
                newName: "Portee");

            migrationBuilder.AddColumn<int>(
                name: "IdEcole",
                table: "Frais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodeDevisePrincipale",
                table: "Ecoles",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "IdAnneeScolaire",
                table: "DevoirsADomicile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FraisClasses",
                columns: table => new
                {
                    IdFrais = table.Column<int>(type: "int", nullable: false),
                    IdClasse = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraisClasses", x => new { x.IdFrais, x.IdClasse });
                    table.ForeignKey(
                        name: "FK_FraisClasses_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                    table.ForeignKey(
                        name: "FK_FraisClasses_Frais_IdFrais",
                        column: x => x.IdFrais,
                        principalTable: "Frais",
                        principalColumn: "IdFrais",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FraisDirections",
                columns: table => new
                {
                    IdFrais = table.Column<int>(type: "int", nullable: false),
                    IdDirection = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraisDirections", x => new { x.IdFrais, x.IdDirection });
                    table.ForeignKey(
                        name: "FK_FraisDirections_Directions_IdDirection",
                        column: x => x.IdDirection,
                        principalTable: "Directions",
                        principalColumn: "IdDirection");
                    table.ForeignKey(
                        name: "FK_FraisDirections_Frais_IdFrais",
                        column: x => x.IdFrais,
                        principalTable: "Frais",
                        principalColumn: "IdFrais",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_Ecole_Annee_Libelle",
                table: "Frais",
                columns: new[] { "IdEcole", "IdAnneeScolaire", "LibelleFrais" });

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_Classe_Annee_Statut",
                table: "DevoirsADomicile",
                columns: new[] { "IdClasse", "IdAnneeScolaire", "Statut" });

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_IdAnneeScolaire",
                table: "DevoirsADomicile",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_FraisClasses_IdClasse",
                table: "FraisClasses",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_FraisDirections_IdDirection",
                table: "FraisDirections",
                column: "IdDirection");

            migrationBuilder.AddForeignKey(
                name: "FK_DevoirsADomicile_AnneeScolaires_IdAnneeScolaire",
                table: "DevoirsADomicile",
                column: "IdAnneeScolaire",
                principalTable: "AnneeScolaires",
                principalColumn: "IdAnneeScolaire",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Frais_Ecoles_IdEcole",
                table: "Frais",
                column: "IdEcole",
                principalTable: "Ecoles",
                principalColumn: "IdEcole");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DevoirsADomicile_AnneeScolaires_IdAnneeScolaire",
                table: "DevoirsADomicile");

            migrationBuilder.DropForeignKey(
                name: "FK_Frais_Ecoles_IdEcole",
                table: "Frais");

            migrationBuilder.DropTable(
                name: "FraisClasses");

            migrationBuilder.DropTable(
                name: "FraisDirections");

            migrationBuilder.DropIndex(
                name: "IX_Frais_Ecole_Annee_Libelle",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_DevoirADomicile_Classe_Annee_Statut",
                table: "DevoirsADomicile");

            migrationBuilder.DropIndex(
                name: "IX_DevoirADomicile_IdAnneeScolaire",
                table: "DevoirsADomicile");

            migrationBuilder.DropColumn(
                name: "IdEcole",
                table: "Frais");

            migrationBuilder.DropColumn(
                name: "CodeDevisePrincipale",
                table: "Ecoles");

            migrationBuilder.DropColumn(
                name: "IdAnneeScolaire",
                table: "DevoirsADomicile");

            migrationBuilder.RenameColumn(
                name: "Portee",
                table: "Frais",
                newName: "IdDirection");

            migrationBuilder.AddColumn<int>(
                name: "IdClasse",
                table: "Frais",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Frais_Direction_Annee_Libelle_Classe",
                table: "Frais",
                columns: new[] { "IdDirection", "IdAnneeScolaire", "LibelleFrais", "IdClasse" });

            migrationBuilder.CreateIndex(
                name: "IX_Frais_IdClasse",
                table: "Frais",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_IdDirection",
                table: "Frais",
                column: "IdDirection");

            migrationBuilder.AddForeignKey(
                name: "FK_Frais_Classes_IdClasse",
                table: "Frais",
                column: "IdClasse",
                principalTable: "Classes",
                principalColumn: "IdClasse");

            migrationBuilder.AddForeignKey(
                name: "FK_Frais_Directions_IdDirection",
                table: "Frais",
                column: "IdDirection",
                principalTable: "Directions",
                principalColumn: "IdDirection");
        }
    }
}
