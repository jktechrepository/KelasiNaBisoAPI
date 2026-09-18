using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Module Dépenses V1 : CategoriesDepense + Depenses.
    /// Migration manuelle (évite dotnet ef long / interrompu).
    /// </summary>
    public partial class AddDepensesModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriesDepense",
                columns: table => new
                {
                    IdCategorieDepense = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    NomCategorie = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesDepense", x => x.IdCategorieDepense);
                    table.ForeignKey(
                        name: "FK_CategoriesDepense_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Depenses",
                columns: table => new
                {
                    IdDepense = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdCategorieDepense = table.Column<int>(type: "int", nullable: false),
                    Libelle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Beneficiaire = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferencePiece = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CodeDeviseMontant = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeDevisePrincipale = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TauxVersDevisePrincipale = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    MontantDevisePrincipale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ModePaiement = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDepense = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StatutWorkflow = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Actif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdUtilisateurCreateur = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MotifAnnulation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdUtilisateurAnnulation = table.Column<int>(type: "int", nullable: true),
                    DateAnnulation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depenses", x => x.IdDepense);
                    table.ForeignKey(
                        name: "FK_Depenses_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Depenses_CategoriesDepense_IdCategorieDepense",
                        column: x => x.IdCategorieDepense,
                        principalTable: "CategoriesDepense",
                        principalColumn: "IdCategorieDepense",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Depenses_Utilisateurs_IdUtilisateurCreateur",
                        column: x => x.IdUtilisateurCreateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriesDepense_IdEcole",
                table: "CategoriesDepense",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Depenses_IdEcole_DateDepense",
                table: "Depenses",
                columns: new[] { "IdEcole", "DateDepense" });

            migrationBuilder.CreateIndex(
                name: "IX_Depenses_IdCategorieDepense",
                table: "Depenses",
                column: "IdCategorieDepense");

            migrationBuilder.CreateIndex(
                name: "IX_Depenses_StatutWorkflow",
                table: "Depenses",
                column: "StatutWorkflow");

            migrationBuilder.CreateIndex(
                name: "IX_Depenses_IdUtilisateurCreateur",
                table: "Depenses",
                column: "IdUtilisateurCreateur");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Depenses");
            migrationBuilder.DropTable(name: "CategoriesDepense");
        }
    }
}
