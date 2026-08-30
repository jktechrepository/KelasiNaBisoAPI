using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class FraisAnneeClasseTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly FraisService _service;
        private readonly DateTime _now = DateTime.Now;

        public FraisAnneeClasseTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new FraisService(_context, scope);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.AddRange(
                TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1),
                TestDataBuilder.CreateClasse(11, "5e B", idDirection: 1));
            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "Courante", _now.AddMonths(-3), _now.AddMonths(6)),
                TestDataBuilder.CreateAnneeScolaire(99, 1, "Past", _now.AddYears(-2), _now.AddYears(-1)));

            _context.Frais.AddRange(
                new Frais
                {
                    IdFrais = 1,
                    LibelleFrais = "Inscription",
                    Montant = 50,
                    Devise = "USD",
                    IdDirection = 1,
                    IdAnneeScolaire = 100,
                    IdClasse = null,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Frais
                {
                    IdFrais = 2,
                    LibelleFrais = "Minerval 6e",
                    Montant = 100,
                    Devise = "USD",
                    IdDirection = 1,
                    IdAnneeScolaire = 100,
                    IdClasse = 10,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Frais
                {
                    IdFrais = 3,
                    LibelleFrais = "Minerval 5e",
                    Montant = 90,
                    Devise = "USD",
                    IdDirection = 1,
                    IdAnneeScolaire = 100,
                    IdClasse = 11,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Frais
                {
                    IdFrais = 4,
                    LibelleFrais = "Inscription Past",
                    Montant = 40,
                    Devise = "USD",
                    IdDirection = 1,
                    IdAnneeScolaire = 99,
                    IdClasse = null,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByEcoleAsync_DefaultsToCurrentYear()
        {
            var result = await _service.GetByEcoleAsync(1, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.Data.Should().HaveCount(3);
            result.Data.Should().NotContain(f => f.IdFrais == 4);
        }

        [Fact]
        public async Task GetByEcoleAsync_WithClasse_ReturnsSharedAndClassFees()
        {
            var result = await _service.GetByEcoleAsync(1, null, idClasse: 10);

            result.Data.Select(f => f.IdFrais).Should().BeEquivalentTo(new[] { 1, 2 });
            result.Data.Should().NotContain(f => f.IdFrais == 3);
        }

        [Fact]
        public async Task CreateAsync_WithoutAnnee_DefaultsToCurrentYear()
        {
            var created = await _service.CreateAsync(new Frais
            {
                LibelleFrais = "Nouveau défaut",
                Montant = 10,
                Devise = "USD",
                IdDirection = 1,
                IdAnneeScolaire = 0,
                Statut = true
            });

            created.IdAnneeScolaire.Should().Be(100);
        }

        [Fact]
        public async Task CreateAsync_WithExplicitAnnee_KeepsProvidedYear()
        {
            var created = await _service.CreateAsync(new Frais
            {
                LibelleFrais = "Nouveau past",
                Montant = 10,
                Devise = "USD",
                IdDirection = 1,
                IdAnneeScolaire = 99,
                Statut = true
            });

            created.IdAnneeScolaire.Should().Be(99);
        }

        [Fact]
        public void FilterForInscription_IncludesSharedAndMatchingClass()
        {
            var eligible = FraisEligibility
                .FilterForInscription(_context.Frais, 1, 100, 10)
                .Select(f => f.IdFrais)
                .ToList();

            eligible.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        public void Dispose() => _context.Dispose();
    }
}
