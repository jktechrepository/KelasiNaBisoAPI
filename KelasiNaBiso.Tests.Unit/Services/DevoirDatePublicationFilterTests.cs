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
    public class DevoirDatePublicationFilterTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DevoirADomicileService _service;
        private readonly DateTime _jour = new(2026, 3, 10, 8, 30, 0);
        private readonly DateTime _jourPlus7;

        public DevoirDatePublicationFilterTests()
        {
            _jourPlus7 = _jour.AddDays(7);
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
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(6)));
            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(1, "Parent"));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, 1, "Alice"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 101));
            _context.DevoirsADomicile.AddRange(
                CreateDevoir(1, "Devoir J", _jour),
                CreateDevoir(2, "Devoir J+7", _jourPlus7));
            _context.SaveChanges();
        }

        private static DevoirADomicile CreateDevoir(int id, string titre, DateTime datePublication) =>
            new()
            {
                IdDevoirADomicile = id,
                Titre = titre,
                IdEcole = 1,
                IdDirection = 1,
                IdAgent = 1,
                IdClasse = 10,
                IdAnneeScolaire = 101,
                Statut = true,
                DatePublication = datePublication,
                DateCreation = datePublication
            };

        private static PagedRequest DefaultRequest() => new() { PageNumber = 1, PageSize = 15 };

        [Fact]
        public async Task GetByClasseAsync_SansFiltreDate_RetourneLesDeuxDevoirs()
        {
            var devoirs = await _service.GetByClasseAsync(10, 101);

            devoirs.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByClasseAsync_DateDebutEtFinEgalJ_RetourneUniquementJ()
        {
            var devoirs = await _service.GetByClasseAsync(10, 101, _jour.Date, _jour.Date);

            devoirs.Should().HaveCount(1);
            devoirs.Single().IdDevoirADomicile.Should().Be(1);
        }

        [Fact]
        public async Task GetByClassePagedAsync_DateDebutEtFinEgalJ_RetourneUniquementJ()
        {
            var result = await _service.GetByClassePagedAsync(10, DefaultRequest(), 101, _jour, _jour);

            result.TotalRecords.Should().Be(1);
            result.Data.Single().IdDevoirADomicile.Should().Be(1);
        }

        [Fact]
        public async Task GetByAgentPagedAsync_DateDebutEtFinEgalJ_RetourneUniquementJ()
        {
            var result = await _service.GetByAgentPagedAsync(1, DefaultRequest(), 101, null, _jour, _jour);

            result.TotalRecords.Should().Be(1);
            result.Data.Single().IdDevoirADomicile.Should().Be(1);
        }

        [Fact]
        public async Task GetByEcolePagedAsync_SansFiltreDate_RetourneLesDeuxDevoirs()
        {
            var result = await _service.GetByEcolePagedAsync(1, DefaultRequest(), 101);

            result.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPagedAsync_DateDebutEtFinEgalJ_RetourneUniquementJ()
        {
            var result = await _service.GetAllPagedAsync(DefaultRequest(), 101, 1, 10, _jour, _jour);

            result.TotalRecords.Should().Be(1);
            result.Data.Single().IdDevoirADomicile.Should().Be(1);
        }

        [Fact]
        public async Task GetByTuteurPagedAsync_DateDebutEtFinEgalJ_RetourneUniquementJ()
        {
            var result = await _service.GetByTuteurPagedAsync(1, DefaultRequest(), null, _jour, _jour);

            result.TotalRecords.Should().Be(1);
            result.Data.Single().IdDevoirADomicile.Should().Be(1);
        }

        [Fact]
        public async Task GetByClasseAsync_DateDebutPosterieureADateFin_Throws()
        {
            var act = async () => await _service.GetByClasseAsync(
                10, 101, _jour.AddDays(1), _jour);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*dateDebut*");
        }

        public void Dispose() => _context.Dispose();
    }
}
