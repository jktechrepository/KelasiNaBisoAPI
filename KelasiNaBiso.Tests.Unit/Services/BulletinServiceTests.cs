using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class BulletinServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly BulletinService _service;
        private readonly DateTime _now = DateTime.Now;

        public BulletinServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scopeFactory = new Mock<IServiceScopeFactory>();
            _service = new BulletinService(_context, resolver, scopeFactory.Object);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2025-2026", _now.AddMonths(-3), _now.AddMonths(6)));

            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, null, "Alice"),
                TestDataBuilder.CreateEleve(2, null, "Bob"));

            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100));

            _context.Cours.AddRange(
                new Cours { IdCours = 50, NomCours = "Math", IdClasse = 10, Statut = true, DateCreation = DateTime.Now },
                new Cours { IdCours = 51, NomCours = "Francais", IdClasse = 10, Statut = true, DateCreation = DateTime.Now });

            _context.Evaluations.AddRange(
                new Evaluation
                {
                    IdEvaluation = 1,
                    TypeEvaluation = "Interro",
                    TitreEvaluation = "Math 1",
                    Periode = "Trimestre 1",
                    Coefficient = 2,
                    IdCours = 50,
                    IdClasse = 10,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Evaluation
                {
                    IdEvaluation = 2,
                    TypeEvaluation = "Interro",
                    TitreEvaluation = "Math 2",
                    Periode = "Trimestre 1",
                    Coefficient = 1,
                    IdCours = 50,
                    IdClasse = 10,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Evaluation
                {
                    IdEvaluation = 3,
                    TypeEvaluation = "Devoir",
                    TitreEvaluation = "Fr 1",
                    Periode = "Trimestre 1",
                    Coefficient = 1,
                    IdCours = 51,
                    IdClasse = 10,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Evaluation
                {
                    IdEvaluation = 4,
                    TypeEvaluation = "Interro",
                    TitreEvaluation = "Math T2",
                    Periode = "Trimestre 2",
                    Coefficient = 1,
                    IdCours = 50,
                    IdClasse = 10,
                    Statut = true,
                    DateCreation = DateTime.Now
                });

            // Alice: Math (10*2 + 16*1)/3 = 12 ; Fr 14 → générale (12*3 + 14*1)/4 = 12.5
            _context.Notes.AddRange(
                new Note
                {
                    IdNote = 1,
                    NoteObtenue = 10,
                    Appreciation = "",
                    DateEvaluation = _now,
                    IdProfesseur = 1,
                    IdEleve = 1,
                    IdEvaluation = 1,
                    IdAnneeScolaire = 100,
                    Statut = true
                },
                new Note
                {
                    IdNote = 2,
                    NoteObtenue = 16,
                    Appreciation = "",
                    DateEvaluation = _now,
                    IdProfesseur = 1,
                    IdEleve = 1,
                    IdEvaluation = 2,
                    IdAnneeScolaire = 100,
                    Statut = true
                },
                new Note
                {
                    IdNote = 3,
                    NoteObtenue = 14,
                    Appreciation = "",
                    DateEvaluation = _now,
                    IdProfesseur = 1,
                    IdEleve = 1,
                    IdEvaluation = 3,
                    IdAnneeScolaire = 100,
                    Statut = true
                },
                // Bob: Math only 18 → meilleure moyenne
                new Note
                {
                    IdNote = 4,
                    NoteObtenue = 18,
                    Appreciation = "",
                    DateEvaluation = _now,
                    IdProfesseur = 1,
                    IdEleve = 2,
                    IdEvaluation = 1,
                    IdAnneeScolaire = 100,
                    Statut = true
                },
                // Alice Trimestre 2 — ne doit pas compter en T1
                new Note
                {
                    IdNote = 5,
                    NoteObtenue = 20,
                    Appreciation = "",
                    DateEvaluation = _now,
                    IdProfesseur = 1,
                    IdEleve = 1,
                    IdEvaluation = 4,
                    IdAnneeScolaire = 100,
                    Statut = true
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetBulletinEleve_WeightedCourseAverage()
        {
            var bulletin = await _service.GetBulletinEleveAsync(1, 100, "Trimestre 1");

            bulletin.Should().NotBeNull();
            var math = bulletin!.Lignes.Single(l => l.IdCours == 50);
            math.MoyenneCours.Should().BeApproximately(12d, 0.001);
            math.Coefficient.Should().Be(3);
        }

        [Fact]
        public async Task GetBulletinEleve_GeneralAverage_MultiCours()
        {
            var bulletin = await _service.GetBulletinEleveAsync(1, 100, "Trimestre 1");

            bulletin.Should().NotBeNull();
            bulletin!.MoyenneGenerale.Should().BeApproximately(12.5d, 0.001);
            bulletin.Lignes.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetBulletinsClasse_RanksBestFirst()
        {
            var list = await _service.GetBulletinsClasseAsync(10, 100, "Trimestre 1");

            list.Should().HaveCount(2);
            var bob = list.Single(b => b.IdEleve == 2);
            var alice = list.Single(b => b.IdEleve == 1);
            bob.Rang.Should().Be(1);
            alice.Rang.Should().Be(2);
            bob.MoyenneGenerale.Should().BeGreaterThan(alice.MoyenneGenerale!.Value);
        }

        [Fact]
        public async Task GetBulletinEleve_ExcludesOtherPeriode()
        {
            var bulletin = await _service.GetBulletinEleveAsync(1, 100, "Trimestre 1");

            bulletin.Should().NotBeNull();
            bulletin!.Lignes.SelectMany(l => l.Notes).Should().NotContain(n => n.IdEvaluation == 4);
            bulletin.Lignes.SelectMany(l => l.Notes).Should().HaveCount(3);
        }

        public void Dispose() => _context.Dispose();
    }
}
