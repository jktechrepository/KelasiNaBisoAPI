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
    public class PresenceFeuilleAppelTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly PresenceReportingService _service;
        private readonly DateTime _now = DateTime.Now;
        private readonly DateTime _jourAppel;

        public PresenceFeuilleAppelTests()
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
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1);
            var classe = TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1);

            var anneeCourante = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "Courante",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6));
            var anneePast = TestDataBuilder.CreateAnneeScolaire(
                99, 1, "Past",
                debut: _now.AddYears(-2),
                fin: _now.AddYears(-1));

            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent");
            var present = TestDataBuilder.CreateEleve(1, 1, "Present");
            var absent = TestDataBuilder.CreateEleve(2, 1, "Absent");
            var retard = TestDataBuilder.CreateEleve(3, 1, "Retard");
            var elevePastYear = TestDataBuilder.CreateEleve(4, 1, "PastYear");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Classes.Add(classe);
            _context.AnneeScolaires.AddRange(anneeCourante, anneePast);
            _context.Tuteurs.Add(tuteur);
            _context.Eleves.AddRange(present, absent, retard, elevePastYear);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100),
                TestDataBuilder.CreateInscription(3, 3, 1, 10, 100),
                TestDataBuilder.CreateInscription(4, 4, 1, 10, 99));

            _context.Presences.AddRange(
                new Presence
                {
                    IdPresence = 1,
                    IdEleve = 1,
                    TypePresence = "ELEVE",
                    IsPresent = true,
                    HeureArrivee = new TimeSpan(7, 45, 0),
                    DateDuJour = _jourAppel,
                    Statut = true,
                    DateCreation = _now
                },
                new Presence
                {
                    IdPresence = 2,
                    IdEleve = 3,
                    TypePresence = "ELEVE",
                    IsPresent = true,
                    HeureArrivee = new TimeSpan(8, 30, 0),
                    DateDuJour = _jourAppel,
                    Statut = true,
                    DateCreation = _now
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetFeuilleAppelAsync_ReturnsPresentAbsentRetard_ForCurrentYear()
        {
            var result = await _service.GetFeuilleAppelAsync(10, _jourAppel, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.IdEcole.Should().Be(1);
            result.Effectif.Should().Be(3);
            result.Lignes.Should().NotContain(l => l.IdEleve == 4);

            result.Lignes.Should().ContainSingle(l =>
                l.IdEleve == 1 && l.StatutJour == FeuilleAppelStatutJour.Present);
            result.Lignes.Should().ContainSingle(l =>
                l.IdEleve == 2 && l.StatutJour == FeuilleAppelStatutJour.Absent);
            result.Lignes.Should().ContainSingle(l =>
                l.IdEleve == 3 && l.StatutJour == FeuilleAppelStatutJour.Retard);

            result.NbPresents.Should().Be(1);
            result.NbAbsents.Should().Be(1);
            result.NbRetards.Should().Be(1);
        }

        [Fact]
        public async Task GetFeuilleAppelAsync_WithPastAnnee_ReturnsOnlyPastYearEleves()
        {
            var result = await _service.GetFeuilleAppelAsync(10, _jourAppel, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Effectif.Should().Be(1);
            result.Lignes.Should().ContainSingle(l =>
                l.IdEleve == 4 && l.StatutJour == FeuilleAppelStatutJour.Absent);
        }

        [Fact]
        public async Task GetFeuilleAppelAsync_WithoutCurrentYear_Throws()
        {
            foreach (var annee in _context.AnneeScolaires)
            {
                annee.DateDebut = _now.AddYears(-5);
                annee.DateFin = _now.AddYears(-4);
            }
            await _context.SaveChangesAsync();

            var act = async () => await _service.GetFeuilleAppelAsync(10, _jourAppel, null);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Aucune année scolaire en cours*");
        }

        public void Dispose() => _context.Dispose();
    }
}
