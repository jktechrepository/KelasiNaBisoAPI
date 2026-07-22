using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddDevoirADomicileAndTelechargements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevoirsADomicile",
                columns: table => new
                {
                    IdDevoirADomicile = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomFichier = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheminFichier = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TailleFichier = table.Column<long>(type: "bigint", nullable: false),
                    TypeMIME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdDirection = table.Column<int>(type: "int", nullable: false),
                    IdAgent = table.Column<int>(type: "int", nullable: false),
                    IdClasse = table.Column<int>(type: "int", nullable: false),
                    IdCours = table.Column<int>(type: "int", nullable: true),
                    DatePublication = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateLimite = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NombreTelechargements = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevoirsADomicile", x => x.IdDevoirADomicile);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicile_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicile_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicile_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicile_Directions_IdDirection",
                        column: x => x.IdDirection,
                        principalTable: "Directions",
                        principalColumn: "IdDirection",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicile_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DevoirsADomicileTelechargements",
                columns: table => new
                {
                    IdDevoirADomicileTelechargement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdDevoirADomicile = table.Column<int>(type: "int", nullable: false),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    DateTelechargement = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevoirsADomicileTelechargements", x => x.IdDevoirADomicileTelechargement);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicileTelechargements_DevoirsADomicile_IdDevoirADo~",
                        column: x => x.IdDevoirADomicile,
                        principalTable: "DevoirsADomicile",
                        principalColumn: "IdDevoirADomicile",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevoirsADomicileTelechargements_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_Classe_Statut",
                table: "DevoirsADomicile",
                columns: new[] { "IdClasse", "Statut" });

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_DatePublication",
                table: "DevoirsADomicile",
                column: "DatePublication");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_IdAgent",
                table: "DevoirsADomicile",
                column: "IdAgent");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_IdClasse",
                table: "DevoirsADomicile",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_IdDirection",
                table: "DevoirsADomicile",
                column: "IdDirection");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicile_IdEcole",
                table: "DevoirsADomicile",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirsADomicile_IdCours",
                table: "DevoirsADomicile",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicileTelechargement_Devoir_Utilisateur",
                table: "DevoirsADomicileTelechargements",
                columns: new[] { "IdDevoirADomicile", "IdUtilisateur" });

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicileTelechargement_IdDevoir",
                table: "DevoirsADomicileTelechargements",
                column: "IdDevoirADomicile");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirsADomicileTelechargements_IdUtilisateur",
                table: "DevoirsADomicileTelechargements",
                column: "IdUtilisateur");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevoirsADomicileTelechargements");

            migrationBuilder.DropTable(
                name: "DevoirsADomicile");
        }
    }
}
