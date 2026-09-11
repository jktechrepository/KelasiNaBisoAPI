using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DevoirTuteurTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DevoirADomicileService _service;
        private readonly DateTime _now = DateTime.Now;

        public DevoirTuteurTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new DevoirADomicileService(
                _context,
                Mock.Of<ICurrentUserService>(),
                NullLogger<DevoirADomicileService>.Instance,
                inscriptionResolver,
                scope);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.AddRange(
                TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1),
                TestDataBuilder.CreateClasse(11, "5e B", idDirection: 1));
            _context.Agents.Add(TestDataBuilder.CreateAgent(1, "Prof", idEcole: 1));

            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025",
                    _now.AddYears(-1), _now.AddMonths(-6)),
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6)));

            _context.Tuteurs.AddRange(
                TestDataBuilder.CreateTuteur(1, "Parent Multi"),
                TestDataBuilder.CreateTuteur(2, "Parent MemeClasse"));

            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, 1, "Alice"),
                TestDataBuilder.CreateEleve(2, 1, "Bob"),
                TestDataBuilder.CreateEleve(3, 2, "Claire"),
                TestDataBuilder.CreateEleve(4, 2, "Denis"));

            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 101),
                TestDataBuilder.CreateInscription(2, 2, 1, 11, 101),
                TestDataBuilder.CreateInscription(3, 3, 1, 10, 101),
                TestDataBuilder.CreateInscription(4, 4, 1, 10, 101),
                TestDataBuilder.CreateInscription(5, 1, 1, 10, 100));

            _context.DevoirsADomicile.AddRange(
                CreateDevoir(1, 10, 101, "Maths 6e"),
                CreateDevoir(2, 11, 101, "Francais 5e"),
                CreateDevoir(3, 10, 100, "Histoire N-1"));

            _context.SaveChanges();
        }

        private static DevoirADomicile CreateDevoir(int id, int idClasse, int idAnnee, string titre) =>
            new()
            {
                IdDevoirADomicile = id,
                Titre = titre,
                Description = $"Desc {titre}",
                IdEcole = 1,
                IdDirection = 1,
                IdAgent = 1,
                IdClasse = idClasse,
                IdAnneeScolaire = idAnnee,
                Statut = true,
                DatePublication = DateTime.Now,
                DateCreation = DateTime.Now
            };

        private static PagedRequest DefaultRequest() => new() { PageNumber = 1, PageSize = 15 };

        [Fact]
        public async Task GetByTuteurPagedAsync_TwoChildrenTwoClasses_ReturnsTwoDevoirsWithMatchingEleves()
        {
            var result = await _service.GetByTuteurPagedAsync(1, DefaultRequest());

            result.Data.Should().HaveCount(2);

            var maths = result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 1).Subject;
            maths.ElevesConcernes.Should().ContainSingle(e => e.IdEleve == 1);
            maths.NomClasse.Should().Be("6e A");
            maths.LibelleAnneeScolaire.Should().Be("2025-2026");

            var francais = result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 2).Subject;
            francais.ElevesConcernes.Should().ContainSingle(e => e.IdEleve == 2);
            francais.NomClasse.Should().Be("5e B");
        }

        [Fact]
        public async Task GetByTuteurPagedAsync_TwoChildrenSameClass_ReturnsOneDevoirWithBothEleves()
        {
            var result = await _service.GetByTuteurPagedAsync(2, DefaultRequest());

            result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 1);
            var devoir = result.Data.Single();
            devoir.ElevesConcernes.Should().HaveCount(2);
            devoir.ElevesConcernes.Select(e => e.IdEleve).Should().BeEquivalentTo(new[] { 3, 4 });
        }

        [Fact]
        public async Task GetByTuteurPagedAsync_PastYear_ReturnsOnlyPastDevoirs()
        {
            var result = await _service.GetByTuteurPagedAsync(
                1, DefaultRequest(), libelleAnneeScolaire: "2024-2025");

            result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 3);
            result.Data.Should().NotContain(d => d.IdDevoirADomicile == 1);
        }

        [Fact]
        public async Task GetByTuteurPagedAsync_LibelleAnneeScolaire_ReturnsMatchingYear()
        {
            var result = await _service.GetByTuteurPagedAsync(
                1, DefaultRequest(), libelleAnneeScolaire: "2024-2025");

            result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 3);
        }

        [Fact]
        public async Task GetByTuteurPagedAsync_SearchTermOnTitre_Filters()
        {
            var request = new PagedRequest { PageNumber = 1, PageSize = 15, SearchTerm = "Maths" };

            var result = await _service.GetByTuteurPagedAsync(1, request);

            result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 1);
        }

        [Fact]
        public async Task GetByTuteurPagedAsync_SearchTermOnNomEleve_Filters()
        {
            var request = new PagedRequest { PageNumber = 1, PageSize = 15, SearchTerm = "Alice" };

            var result = await _service.GetByTuteurPagedAsync(1, request);

            result.Data.Should().ContainSingle(d => d.IdDevoirADomicile == 1);
            result.Data.Single().ElevesConcernes.Should().ContainSingle(e => e.IdEleve == 1);
        }

        public void Dispose() => _context.Dispose();
    }
}
