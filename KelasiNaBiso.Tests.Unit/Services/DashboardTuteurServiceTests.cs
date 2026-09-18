using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DashboardTuteurServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DashboardTuteurService _service;
        private readonly DateTime _now = DateTime.Now;

        public DashboardTuteurServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new DashboardTuteurService(_context, scope);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.AddRange(
                TestDataBuilder.CreateEcole(1, "Ecole Alpha"),
                TestDataBuilder.CreateEcole(2, "Ecole Beta"));
            _context.Directions.AddRange(
                TestDataBuilder.CreateDirection(1, 1),
                TestDataBuilder.CreateDirection(2, 2));
            _context.Classes.AddRange(
                TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1),
                TestDataBuilder.CreateClasse(20, "5e B", idDirection: 2));
            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(11, "Parent Multi"));

            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025",
                    _now.AddYears(-1), _now.AddMonths(-6)),
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6)),
                TestDataBuilder.CreateAnneeScolaire(201, 2, "2025-2026",
                    _now.AddMonths(-2), _now.AddMonths(7)));

            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, 11, "Alice"),
                TestDataBuilder.CreateEleve(2, 11, "Bob"));

            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 101),
                TestDataBuilder.CreateInscription(2, 1, 1, 10, 100, dateInscription: _now.AddYears(-1)),
                TestDataBuilder.CreateInscription(3, 2, 2, 20, 201));

            _context.Frais.AddRange(
                TestDataBuilder.CreateFrais(1, 1, 100, "Frais N-1", montant: 50),
                TestDataBuilder.CreateFrais(2, 1, 101, "Frais N", montant: 120),
                TestDataBuilder.CreateFrais(3, 2, 201, "Frais Beta", montant: 80));

            _context.Paiements.AddRange(
                new Paiement
                {
                    IdPaiement = 1,
                    IdEleve = 1,
                    IdFrais = 1,
                    Montant = 50,
                    Devise = "USD",
                    DatePaiement = _now.AddMonths(-8),
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "OLD"
                },
                new Paiement
                {
                    IdPaiement = 2,
                    IdEleve = 1,
                    IdFrais = 2,
                    Montant = 40,
                    Devise = "USD",
                    DatePaiement = _now.AddDays(-5),
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "CUR"
                },
                new Paiement
                {
                    IdPaiement = 3,
                    IdEleve = 2,
                    IdFrais = 3,
                    Montant = 30,
                    Devise = "USD",
                    DatePaiement = _now.AddDays(-2),
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "BETA"
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task BuildAsync_Mono_ExcludesPaiementsAnneePrecedente()
        {
            var dto = await _service.BuildAsync(11, idEcole: 1, libelleAnneeScolaire: "2025-2026");

            dto.Ecole.Should().NotBeNull();
            dto.Ecole!.IdEcole.Should().Be(1);
            dto.IdAnneeScolaire.Should().Be(101);
            dto.Enfants.Should().HaveCount(1);
            dto.Enfants[0].IdEleve.Should().Be(1);
            dto.Enfants[0].NombrePaiements.Should().Be(1);
            dto.Enfants[0].MontantPaye.Should().Be(40m);
            dto.Resume.MontantTotalPaye.Should().Be(40m);
        }

        [Fact]
        public async Task BuildAsync_Multi_SansIdEcole_AgregeToutesLesEcolesCourantes()
        {
            var dto = await _service.BuildAsync(11, idEcole: null, libelleAnneeScolaire: null);

            dto.Ecole.Should().BeNull();
            dto.IdAnneeScolaire.Should().BeNull();
            dto.Ecoles.Should().HaveCount(2);
            dto.Ecoles.Select(e => e.IdEcole).Should().BeEquivalentTo(new[] { 1, 2 });
            dto.Enfants.Should().HaveCount(2);
            dto.Resume.MontantTotalPaye.Should().Be(70m);

            var alice = dto.Enfants.Single(e => e.IdEleve == 1);
            alice.NombrePaiements.Should().Be(1);
            alice.MontantPaye.Should().Be(40m);
        }

        [Fact]
        public async Task BuildAsync_Multi_AvecLibelle_FiltreParLibelle()
        {
            var dto = await _service.BuildAsync(11, idEcole: null, libelleAnneeScolaire: "2025-2026");

            dto.LibelleAnneeScolaire.Should().Be("2025-2026");
            dto.Enfants.Should().HaveCount(2);
            dto.Ecole.Should().BeNull();
        }

        public void Dispose() => _context.Dispose();
    }
}
