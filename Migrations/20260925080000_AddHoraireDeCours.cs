using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using KelasiNaBiso.Data;

#nullable disable

namespace KelasiNaBiso.Migrations;

[DbContext(typeof(KelasiNaBisoDbContext))]
[Migration("20260925080000_AddHoraireDeCours")]
public partial class AddHoraireDeCours : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "HorairesDeCours",
            columns: table => new
            {
                IdHoraireDeCours = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Jour = table.Column<int>(type: "int", nullable: false),
                HeureDebut = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                HeureFin = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                IdCours = table.Column<int>(type: "int", nullable: false),
                IdClasse = table.Column<int>(type: "int", nullable: false),
                IdAgent = table.Column<int>(type: "int", nullable: false),
                IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                Salle = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HorairesDeCours", x => x.IdHoraireDeCours);
                table.ForeignKey("FK_HorairesDeCours_Agents_IdAgent", x => x.IdAgent, "Agents", "IdAgent", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_HorairesDeCours_AnneeScolaires_IdAnneeScolaire", x => x.IdAnneeScolaire, "AnneeScolaires", "IdAnneeScolaire", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_HorairesDeCours_Classes_IdClasse", x => x.IdClasse, "Classes", "IdClasse", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_HorairesDeCours_Cours_IdCours", x => x.IdCours, "Cours", "IdCours", onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_HorairesDeCours_IdAgent_IdAnneeScolaire_Jour_HeureDebut",
            table: "HorairesDeCours",
            columns: new[] { "IdAgent", "IdAnneeScolaire", "Jour", "HeureDebut" });

        migrationBuilder.CreateIndex(
            name: "IX_HorairesDeCours_IdClasse_IdAnneeScolaire_Jour_HeureDebut",
            table: "HorairesDeCours",
            columns: new[] { "IdClasse", "IdAnneeScolaire", "Jour", "HeureDebut" });

        migrationBuilder.CreateIndex(
            name: "IX_HorairesDeCours_IdCours",
            table: "HorairesDeCours",
            column: "IdCours");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "HorairesDeCours");
}
