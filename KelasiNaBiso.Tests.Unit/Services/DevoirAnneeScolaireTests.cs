using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DevoirAnneeScolaireTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DevoirADomicileService _service;
        private readonly DateTime _now = DateTime.Now;

        public DevoirAnneeScolaireTests()
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
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.Agents.Add(TestDataBuilder.CreateAgent(1, "Prof", idEcole: 1));

            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025",
                    _now.AddYears(-1), _now.AddMonths(-6)),
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6)));

            _context.DevoirsADomicile.AddRange(
                CreateDevoir(1, 100, "Devoir N-1"),
                CreateDevoir(2, 101, "Devoir N"));

            _context.SaveChanges();
        }

        private static DevoirADomicile CreateDevoir(int id, int idAnnee, string titre) =>
            new()
            {
                IdDevoirADomicile = id,
                Titre = titre,
                IdEcole = 1,
                IdDirection = 1,
                IdAgent = 1,
                IdClasse = 10,
                IdAnneeScolaire = idAnnee,
                Statut = true,
                DatePublication = DateTime.Now,
                DateCreation = DateTime.Now
            };

        [Fact]
        public async Task GetByClasseAsync_CurrentYear_ReturnsOnlyCurrentYearDevoirs()
        {
            var devoirs = await _service.GetByClasseAsync(10, 101);

            devoirs.Should().HaveCount(1);
            devoirs.Single().IdDevoirADomicile.Should().Be(2);
        }

        [Fact]
        public async Task GetByClasseAsync_ExplicitPastYear_ReturnsPastDevoirs()
        {
            var devoirs = await _service.GetByClasseAsync(10, 100);

            devoirs.Should().HaveCount(1);
            devoirs.Single().IdDevoirADomicile.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_StoresAnneeScolaire()
        {
            var devoir = new DevoirADomicile
            {
                Titre = "Nouveau devoir",
                IdEcole = 1,
                IdDirection = 1,
                IdAgent = 1,
                IdClasse = 10,
                IdAnneeScolaire = 101
            };

            var created = await _service.CreateAsync(devoir);

            created.IdAnneeScolaire.Should().Be(101);
            var fromDb = await _service.GetByIdAsync(created.IdDevoirADomicile);
            fromDb.IdAnneeScolaire.Should().Be(101);
        }

        [Fact]
        public async Task GetByEcolePagedAsync_FiltersByYear()
        {
            var result = await _service.GetByEcolePagedAsync(
                1,
                new Models.DTOs.Pagination.PagedRequest { PageNumber = 1, PageSize = 10 },
                101);

            result.Data.Should().HaveCount(1);
            result.Data.Single().IdDevoirADomicile.Should().Be(2);
        }

        public void Dispose() => _context.Dispose();
    }
}
