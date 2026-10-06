using FluentAssertions;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Reporting;
using OfficeOpenXml;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class FeuilleAppelAgentsExcelExporterTests
    {
        [Fact]
        public void Export_PeriodGroupsDailyRowsByAgentId()
        {
            var exporter = new FeuilleAppelAgentsExcelExporter();
            var feuille = new FeuilleAppelAgentsPeriodeDto
            {
                IdEcole = 1,
                NomEcole = "Ecole Test",
                DateDebut = new DateTime(2026, 9, 1),
                DateFin = new DateTime(2026, 9, 2),
                Jours =
                {
                    BuildDay(
                        new DateTime(2026, 9, 1),
                        (7, "Agent Seven", FeuilleAppelStatutJour.Present),
                        (2, "Agent Two", FeuilleAppelStatutJour.Retard)),
                    BuildDay(
                        new DateTime(2026, 9, 2),
                        (7, "Agent Seven", FeuilleAppelStatutJour.Absent),
                        (2, "Agent Two", FeuilleAppelStatutJour.Present))
                }
            };

            var bytes = exporter.Export(feuille);
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.Single();

            worksheet.Cells[7, 1].Text.Should().Be("Date");
            worksheet.Cells[8, 1].Text.Should().Be("Agent #2 - Agent Two (Enseignant)");
            worksheet.Cells[9, 1].GetValue<DateTime>().Should().Be(new DateTime(2026, 9, 1));
            worksheet.Cells[9, 7].Text.Should().Be(FeuilleAppelStatutJour.Retard);
            worksheet.Cells[10, 1].GetValue<DateTime>().Should().Be(new DateTime(2026, 9, 2));
            worksheet.Cells[10, 7].Text.Should().Be(FeuilleAppelStatutJour.Present);
            worksheet.Cells[11, 1].Text.Should().Be("Agent #7 - Agent Seven (Enseignant)");
            worksheet.Cells[12, 1].GetValue<DateTime>().Should().Be(new DateTime(2026, 9, 1));
            worksheet.Cells[12, 7].Text.Should().Be(FeuilleAppelStatutJour.Present);
            worksheet.Cells[13, 1].GetValue<DateTime>().Should().Be(new DateTime(2026, 9, 2));
            worksheet.Cells[13, 7].Text.Should().Be(FeuilleAppelStatutJour.Absent);
            exporter.GetFileName(feuille)
                .Should()
                .Be("FeuilleAppel_Agents_Ecole_Test_20260901_20260902.xlsx");
        }

        private static FeuilleAppelAgentsDto BuildDay(
            DateTime date,
            params (int IdAgent, string Name, string Status)[] agents)
        {
            var lignes = agents.Select(agent => new FeuilleAppelAgentLigneDto
            {
                IdAgent = agent.IdAgent,
                Date = date,
                Matricule = $"A{agent.IdAgent:000}",
                NomComplet = agent.Name,
                Fonction = "Enseignant",
                StatutJour = agent.Status
            }).ToList();

            return new FeuilleAppelAgentsDto
            {
                IdEcole = 1,
                NomEcole = "Ecole Test",
                Date = date,
                Effectif = lignes.Count,
                NbPresents = lignes.Count(ligne => ligne.StatutJour == FeuilleAppelStatutJour.Present),
                NbAbsents = lignes.Count(ligne => ligne.StatutJour == FeuilleAppelStatutJour.Absent),
                NbRetards = lignes.Count(ligne => ligne.StatutJour == FeuilleAppelStatutJour.Retard),
                Lignes = lignes
            };
        }
    }
}
