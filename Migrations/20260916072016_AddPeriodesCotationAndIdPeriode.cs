using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddPeriodesCotationAndIdPeriode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPeriode",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PeriodesCotation",
                columns: table => new
                {
                    IdPeriode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Libelle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodesCotation", x => x.IdPeriode);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_IdPeriode",
                table: "Evaluations",
                column: "IdPeriode");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodesCotation_Code",
                table: "PeriodesCotation",
                column: "Code",
                unique: true);

            // Seed T1 / T2 / T3 (idempotent)
            migrationBuilder.Sql(@"
INSERT INTO `PeriodesCotation` (`Code`, `Libelle`, `Ordre`, `Statut`, `DateCreation`)
SELECT 'T1', 'Trimestre 1', 1, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM `PeriodesCotation` WHERE `Code` = 'T1');

INSERT INTO `PeriodesCotation` (`Code`, `Libelle`, `Ordre`, `Statut`, `DateCreation`)
SELECT 'T2', 'Trimestre 2', 2, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM `PeriodesCotation` WHERE `Code` = 'T2');

INSERT INTO `PeriodesCotation` (`Code`, `Libelle`, `Ordre`, `Statut`, `DateCreation`)
SELECT 'T3', 'Trimestre 3', 3, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM `PeriodesCotation` WHERE `Code` = 'T3');
");

            // Backfill Evaluation.IdPeriode + normalise Periode = Libelle
            migrationBuilder.Sql(@"
UPDATE `Evaluations` e
INNER JOIN `PeriodesCotation` p ON p.`Code` = 'T1'
SET e.`IdPeriode` = p.`IdPeriode`, e.`Periode` = p.`Libelle`
WHERE e.`IdPeriode` IS NULL
  AND e.`Periode` IS NOT NULL
  AND LOWER(TRIM(e.`Periode`)) IN (
    't1', 'trimestre 1', '1er trimestre', '1ère trimestre', '1ere trimestre',
    'premier trimestre', '1 trimestre', 'trim 1', '1er trim', '1ère trim', '1ere trim', '1e trimestre'
  );

UPDATE `Evaluations` e
INNER JOIN `PeriodesCotation` p ON p.`Code` = 'T2'
SET e.`IdPeriode` = p.`IdPeriode`, e.`Periode` = p.`Libelle`
WHERE e.`IdPeriode` IS NULL
  AND e.`Periode` IS NOT NULL
  AND LOWER(TRIM(e.`Periode`)) IN (
    't2', 'trimestre 2', '2ème trimestre', '2eme trimestre', 'deuxieme trimestre',
    'deuxième trimestre', '2 trimestre', 'trim 2', '2e trimestre', '2eme trim'
  );

UPDATE `Evaluations` e
INNER JOIN `PeriodesCotation` p ON p.`Code` = 'T3'
SET e.`IdPeriode` = p.`IdPeriode`, e.`Periode` = p.`Libelle`
WHERE e.`IdPeriode` IS NULL
  AND e.`Periode` IS NOT NULL
  AND LOWER(TRIM(e.`Periode`)) IN (
    't3', 'trimestre 3', '3ème trimestre', '3eme trimestre', 'troisieme trimestre',
    'troisième trimestre', '3 trimestre', 'trim 3', '3e trimestre', '3eme trim'
  );
");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_PeriodesCotation_IdPeriode",
                table: "Evaluations",
                column: "IdPeriode",
                principalTable: "PeriodesCotation",
                principalColumn: "IdPeriode",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_PeriodesCotation_IdPeriode",
                table: "Evaluations");

            migrationBuilder.DropTable(
                name: "PeriodesCotation");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_IdPeriode",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "IdPeriode",
                table: "Evaluations");
        }
    }
}
