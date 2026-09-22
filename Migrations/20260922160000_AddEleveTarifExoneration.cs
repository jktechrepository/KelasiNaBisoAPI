using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    [DbContext(typeof(KelasiNaBiso.Data.KelasiNaBisoDbContext))]
    [Migration("20260922160000_AddEleveTarifExoneration")]
    public partial class AddEleveTarifExoneration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriesEleveTarif",
                columns: table => new
                {
                    IdCategorieEleveTarif = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Libelle = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesEleveTarif", x => x.IdCategorieEleveTarif);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UX_CategoriesEleveTarif_IdEcole_Code",
                table: "CategoriesEleveTarif",
                columns: new[] { "IdEcole", "Code" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "AffectationsEleveCategorieTarif",
                columns: table => new
                {
                    IdAffectationEleveCategorieTarif = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdCategorieEleveTarif = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Motif = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdAuteur = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffectationsEleveCategorieTarif", x => x.IdAffectationEleveCategorieTarif);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsEleveCategorieTarif_Eleve_Annee",
                table: "AffectationsEleveCategorieTarif",
                columns: new[] { "IdEleve", "IdAnneeScolaire" });

            migrationBuilder.CreateTable(
                name: "ReglesExonerationFrais",
                columns: table => new
                {
                    IdRegleExonerationFrais = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    IdCategorieEleveTarif = table.Column<int>(type: "int", nullable: false),
                    IdFrais = table.Column<int>(type: "int", nullable: false),
                    TypeRegle = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valeur = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdAuteur = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglesExonerationFrais", x => x.IdRegleExonerationFrais);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UX_ReglesExonerationFrais_Annee_Categorie_Frais",
                table: "ReglesExonerationFrais",
                columns: new[] { "IdAnneeScolaire", "IdCategorieEleveTarif", "IdFrais" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReglesExonerationFrais");
            migrationBuilder.DropTable(name: "AffectationsEleveCategorieTarif");
            migrationBuilder.DropTable(name: "CategoriesEleveTarif");
        }
    }
}
