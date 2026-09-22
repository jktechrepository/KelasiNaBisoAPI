using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PresenceFeuilleAppelAgentsTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly PresenceReportingService _service;
        private readonly DateTime _now = DateTime.Now;
        private readonly DateTime _jourAppel;

        public PresenceFeuilleAppelAgentsTests()
        {
            _jourAppel = _now.Date;
            _context = TestDbContextFactory.CreateInMemoryContext();
            var cache = new Mock<ICacheService>().Object;
            var anneeRepo = new AnneeScolaireService(_context);
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new PresenceReportingService(
                _context,
                cache,
                inscriptionResolver,
                scope);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));

            var present = TestDataBuilder.CreateAgent(1, "Present", idEcole: 1);
            present.Fonction = "Enseignant";
            present.Matricule = "A001";

            var absent = TestDataBuilder.CreateAgent(2, "Absent", idEcole: 1);
            absent.Fonction = "Enseignant";
            absent.Matricule = "A002";

            var retard = TestDataBuilder.CreateAgent(3, "Retard", idEcole: 1);
            retard.Fonction = "Caissier";
            retard.Matricule = "A003";

            var autreEcole = TestDataBuilder.CreateAgent(4, "AutreEcole", idEcole: 2);
            autreEcole.Fonction = "Enseignant";

            var inactif = TestDataBuilder.CreateAgent(5, "Inactif", idEcole: 1, statut: false);
            inactif.Fonction = "Enseignant";

            _context.Agents.AddRange(present, absent, retard, autreEcole, inactif);

            _context.Presences.AddRange(
                new Presence
                {
                    IdPresence = 1,
                    IdAgent = 1,
                    TypePresence = "AGENT",
                    IsPresent = true,
                    HeureArrivee = new TimeSpan(7, 45, 0),
                    DateDuJour = _jourAppel,
                    Statut = true,
                    DateCreation = _now
                },
                new Presence
                {
                    IdPresence = 2,
                    IdAgent = 3,
                    TypePresence = "AGENT",
                    IsPresent = true,
                    HeureArrivee = new TimeSpan(8, 30, 0),
                    DateDuJour = _jourAppel,
                    Statut = true,
                    DateCreation = _now
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetFeuilleAppelAgentsAsync_ReturnsPresentAbsentRetard()
        {
            var result = await _service.GetFeuilleAppelAgentsAsync(1, _jourAppel);

            result.Effectif.Should().Be(3);
            result.Lignes.Should().Contain(l =>
                l.IdAgent == 1 && l.StatutJour == FeuilleAppelStatutJour.Present);
            result.Lignes.Should().Contain(l =>
                l.IdAgent == 2 && l.StatutJour == FeuilleAppelStatutJour.Absent);
            result.Lignes.Should().Contain(l =>
                l.IdAgent == 3 && l.StatutJour == FeuilleAppelStatutJour.Retard);
            result.NbPresents.Should().Be(1);
            result.NbAbsents.Should().Be(1);
            result.NbRetards.Should().Be(1);
        }

        [Fact]
        public async Task GetFeuilleAppelAgentsAsync_WithFonction_FiltersAgents()
        {
            var result = await _service.GetFeuilleAppelAgentsAsync(1, _jourAppel, "Enseignant");

            result.Effectif.Should().Be(2);
            result.FonctionFiltre.Should().Be("Enseignant");
            result.Lignes.Select(l => l.IdAgent).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task GetFeuilleAppelAgentsAsync_UnknownEcole_Throws()
        {
            var act = async () => await _service.GetFeuilleAppelAgentsAsync(999, _jourAppel);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        public void Dispose() => _context.Dispose();
    }
}
