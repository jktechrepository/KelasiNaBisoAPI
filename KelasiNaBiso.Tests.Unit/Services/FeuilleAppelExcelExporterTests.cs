using FluentAssertions;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Reporting;
using OfficeOpenXml;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class FeuilleAppelExcelExporterTests
    {
        private readonly FeuilleAppelExcelExporter _exporter = new();

        [Fact]
        public void Export_MinimalFeuille_ReturnsNonEmptyXlsx()
        {
            var feuille = new FeuilleAppelClasseDto
            {
                IdClasse = 10,
                NomClasse = "6e A",
                IdEcole = 1,
                NomEcole = "Ecole Test",
                IdAnneeScolaire = 100,
                Date = new DateTime(2026, 8, 4),
                Effectif = 2,
                NbPresents = 1,
                NbAbsents = 1,
                NbRetards = 0,
                Lignes =
                {
                    new FeuilleAppelLigneDto
                    {
                        IdEleve = 1,
                        Matricule = "E001",
                        NomComplet = "Eleve Present",
                        Genre = "M",
                        StatutJour = FeuilleAppelStatutJour.Present,
                        HeureArrivee = new TimeSpan(7, 15, 0)
                    },
                    new FeuilleAppelLigneDto
                    {
                        IdEleve = 2,
                        Matricule = "E002",
                        NomComplet = "Eleve Absent",
                        Genre = "F",
                        StatutJour = FeuilleAppelStatutJour.Absent
                    }
                }
            };

            var bytes = _exporter.Export(feuille);

            bytes.Should().NotBeNullOrEmpty();
            _exporter.ContentType.Should().Be(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            _exporter.GetFileName(feuille).Should().Be("FeuilleAppel_6e_A_20260804.xlsx");

            using var package = new ExcelPackage(new MemoryStream(bytes));
            var ws = package.Workbook.Worksheets["Feuille d'appel"];
            ws.Should().NotBeNull();
            ws!.Cells[3, 2].Text.Should().Be("6e A");
            ws.Cells[13, 3].Text.Should().Be("Eleve Present");
            ws.Cells[13, 5].Text.Should().Be(FeuilleAppelStatutJour.Present);
            ws.Cells[14, 5].Text.Should().Be(FeuilleAppelStatutJour.Absent);
        }
    }
}
