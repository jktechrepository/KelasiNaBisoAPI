using FluentAssertions;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Reporting;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class FeuilleAppelPdfReportServiceTests
    {
        private readonly FeuilleAppelPdfReportService _service;

        public FeuilleAppelPdfReportServiceTests()
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
            _service = new FeuilleAppelPdfReportService(
                env.Object,
                NullLogger<FeuilleAppelPdfReportService>.Instance);
        }

        [Fact]
        public void MapLignesEleves_FormatsIndexAndHours()
        {
            var feuille = BuildFeuilleEleves();
            var lignes = FeuilleAppelPdfReportService.MapLignesEleves(feuille);

            lignes.Should().HaveCount(3);
            lignes[0].Index.Should().Be("1");
            lignes[0].StatutJour.Should().Be(FeuilleAppelStatutJour.Present);
            lignes[0].HeureArrivee.Should().Be("07:15");
            lignes[1].StatutJour.Should().Be(FeuilleAppelStatutJour.Absent);
            lignes[1].HeureArrivee.Should().BeEmpty();
            lignes[2].StatutJour.Should().Be(FeuilleAppelStatutJour.Retard);
            lignes[2].HeureArrivee.Should().Be("08:30");
        }

        [Fact]
        public void MapLignesAgents_IncludesFonction()
        {
            var feuille = BuildFeuilleAgents();
            var lignes = FeuilleAppelPdfReportService.MapLignesAgents(feuille);

            lignes.Should().HaveCount(2);
            lignes[0].Fonction.Should().Be("Enseignant");
            lignes[1].Fonction.Should().Be("Caissier");
        }

        [Fact]
        public void ExportEleves_ReturnsNonEmptyPdf()
        {
            if (!FastReportPdfTestEnvironment.IsAvailable)
                return;

            var feuille = BuildFeuilleEleves();
            var bytes = _service.ExportEleves(feuille);

            bytes.Should().NotBeNullOrEmpty();
            bytes.Length.Should().BeGreaterThan(100);
            bytes[0].Should().Be(0x25); // '%'
            bytes[1].Should().Be(0x50); // 'P'
            bytes[2].Should().Be(0x44); // 'D'
            bytes[3].Should().Be(0x46); // 'F'
            _service.GetFileNameEleves(feuille).Should().Be("FeuilleAppel_6e_A_20260804.pdf");
            _service.ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public void ExportAgents_ReturnsNonEmptyPdf()
        {
            if (!FastReportPdfTestEnvironment.IsAvailable)
                return;

            var feuille = BuildFeuilleAgents();
            var bytes = _service.ExportAgents(feuille);

            bytes.Should().NotBeNullOrEmpty();
            bytes.Length.Should().BeGreaterThan(100);
            bytes[0].Should().Be(0x25);
            bytes[1].Should().Be(0x50);
            bytes[2].Should().Be(0x44);
            bytes[3].Should().Be(0x46);
            _service.GetFileNameAgents(feuille).Should().Be("FeuilleAppel_Agents_Ecole_Test_Enseignant_20260804.pdf");
        }

        [Fact]
        public void GetFileNameEleves_SansExport_ToujoursCorrect()
        {
            var feuille = BuildFeuilleEleves();
            _service.GetFileNameEleves(feuille).Should().Be("FeuilleAppel_6e_A_20260804.pdf");
            _service.ContentType.Should().Be("application/pdf");
        }

        private static FeuilleAppelClasseDto BuildFeuilleEleves() =>
            new()
            {
                IdClasse = 10,
                NomClasse = "6e A",
                IdEcole = 1,
                NomEcole = "Ecole Test",
                IdAnneeScolaire = 100,
                Date = new DateTime(2026, 8, 4),
                Effectif = 3,
                NbPresents = 1,
                NbAbsents = 1,
                NbRetards = 1,
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
                    },
                    new FeuilleAppelLigneDto
                    {
                        IdEleve = 3,
                        Matricule = "E003",
                        NomComplet = "Eleve Retard",
                        Genre = "M",
                        StatutJour = FeuilleAppelStatutJour.Retard,
                        HeureArrivee = new TimeSpan(8, 30, 0)
                    }
                }
            };

        private static FeuilleAppelAgentsDto BuildFeuilleAgents() =>
            new()
            {
                IdEcole = 1,
                NomEcole = "Ecole Test",
                FonctionFiltre = "Enseignant",
                Date = new DateTime(2026, 8, 4),
                Effectif = 2,
                NbPresents = 1,
                NbAbsents = 1,
                NbRetards = 0,
                Lignes =
                {
                    new FeuilleAppelAgentLigneDto
                    {
                        IdAgent = 1,
                        Matricule = "A001",
                        NomComplet = "Agent Present",
                        Fonction = "Enseignant",
                        Genre = "M",
                        StatutJour = FeuilleAppelStatutJour.Present,
                        HeureArrivee = new TimeSpan(7, 45, 0)
                    },
                    new FeuilleAppelAgentLigneDto
                    {
                        IdAgent = 2,
                        Matricule = "A002",
                        NomComplet = "Agent Absent",
                        Fonction = "Caissier",
                        Genre = "F",
                        StatutJour = FeuilleAppelStatutJour.Absent
                    }
                }
            };
    }
}
