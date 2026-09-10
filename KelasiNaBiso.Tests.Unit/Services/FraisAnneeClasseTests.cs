using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
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
                TestDataBuilder.CreateClasse(11, "5e B", idDirection: 1),
                TestDataBuilder.CreateClasse(12, "4e C", idDirection: 1));
            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "Courante", _now.AddMonths(-3), _now.AddMonths(6)),
                TestDataBuilder.CreateAnneeScolaire(99, 1, "Past", _now.AddYears(-2), _now.AddYears(-1)));

            _context.Frais.AddRange(
                TestDataBuilder.CreateFrais(1, 1, 100, "Inscription", PorteeFrais.Direction, 50),
                TestDataBuilder.CreateFrais(2, 1, 100, "Minerval 6e", PorteeFrais.Classe, 100),
                TestDataBuilder.CreateFrais(3, 1, 100, "Minerval 5e", PorteeFrais.Classe, 90),
                TestDataBuilder.CreateFrais(4, 1, 99, "Inscription Past", PorteeFrais.Direction, 40));

            _context.FraisDirections.Add(TestDataBuilder.CreateFraisDirection(1, 1));
            _context.FraisDirections.Add(TestDataBuilder.CreateFraisDirection(4, 1));
            _context.FraisClasses.Add(TestDataBuilder.CreateFraisClasse(2, 10));
            _context.FraisClasses.Add(TestDataBuilder.CreateFraisClasse(3, 11));
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
            var created = await _service.CreateAsync(new CreateFraisDto
            {
                LibelleFrais = "Nouveau défaut",
                Montant = 10,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 0,
                Portee = PorteeFrais.Direction,
                IdDirections = new List<int> { 1 },
                Statut = true
            });

            created.IdAnneeScolaire.Should().Be(100);
            created.Directions.Should().ContainSingle(d => d.IdDirection == 1);
        }

        [Fact]
        public async Task CreateAsync_WithExplicitAnnee_KeepsProvidedYear()
        {
            var created = await _service.CreateAsync(new CreateFraisDto
            {
                LibelleFrais = "Nouveau past",
                Montant = 10,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 99,
                Portee = PorteeFrais.Direction,
                IdDirections = new List<int> { 1 },
                Statut = true
            });

            created.IdAnneeScolaire.Should().Be(99);
        }

        [Fact]
        public async Task CreateAsync_PorteeClasse_MultipleClasses()
        {
            var created = await _service.CreateAsync(new CreateFraisDto
            {
                LibelleFrais = "Transport 6e et 4e",
                Montant = 20,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 100,
                Portee = PorteeFrais.Classe,
                IdClasses = new List<int> { 10, 12 },
                Statut = true
            });

            created.Portee.Should().Be(PorteeFrais.Classe);
            created.Classes.Select(c => c.IdClasse).Should().BeEquivalentTo(new[] { 10, 12 });
            created.Directions.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_PorteeDirection_WithClasses_Throws()
        {
            var act = () => _service.CreateAsync(new CreateFraisDto
            {
                LibelleFrais = "XOR invalide",
                Montant = 10,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 100,
                Portee = PorteeFrais.Direction,
                IdDirections = new List<int> { 1 },
                IdClasses = new List<int> { 10 },
                Statut = true
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*ne peut pas lister des classes*");
        }

        [Fact]
        public void FilterForInscription_IncludesSharedAndMatchingClass()
        {
            var eligible = FraisEligibility
                .FilterForInscription(_context.Frais, 1, 1, 100, 10)
                .Select(f => f.IdFrais)
                .ToList();

            eligible.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        public void Dispose() => _context.Dispose();
    }
}
