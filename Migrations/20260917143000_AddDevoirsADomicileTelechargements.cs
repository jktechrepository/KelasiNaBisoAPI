using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Suivi des téléchargements devoir × utilisateur (estTelechargeParMoi).
    /// Migration manuelle.
    /// </summary>
    public partial class AddDevoirsADomicileTelechargements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        name: "FK_DevoirTelechargements_Devoir",
                        column: x => x.IdDevoirADomicile,
                        principalTable: "DevoirsADomicile",
                        principalColumn: "IdDevoirADomicile",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevoirTelechargements_Utilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DevoirADomicileTelechargement_Devoir_Utilisateur",
                table: "DevoirsADomicileTelechargements",
                columns: new[] { "IdDevoirADomicile", "IdUtilisateur" },
                unique: true);

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
            migrationBuilder.DropTable(name: "DevoirsADomicileTelechargements");
        }
    }
}
