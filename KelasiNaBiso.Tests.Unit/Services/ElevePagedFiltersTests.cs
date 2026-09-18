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
    public class ElevePagedFiltersTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _service;
        private readonly DateTime _now = DateTime.Now;

        public ElevePagedFiltersTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionRepo = new Mock<IInscriptionRepository>().Object;
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new EleveService(
                _context,
                inscriptionRepo,
                inscriptionResolver,
                scope,
                NullLogger<EleveService>.Instance);
            Seed();
        }

        private void Seed()
        {
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction1 = TestDataBuilder.CreateDirection(1, 1, "Primaire");
            var direction2 = TestDataBuilder.CreateDirection(2, 1, "Secondaire");
            var classe10 = TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1);
            var classe20 = TestDataBuilder.CreateClasse(20, "1ère B", idDirection: 2);
            var anneeCourante = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "Courante",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6));

            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent");
            var eleveClasse10 = TestDataBuilder.CreateEleve(1, 1, "Classe10");
            var eleveClasse20 = TestDataBuilder.CreateEleve(2, 1, "Classe20");

            _context.Ecoles.Add(ecole);
            _context.Directions.AddRange(direction1, direction2);
            _context.Classes.AddRange(classe10, classe20);
            _context.AnneeScolaires.Add(anneeCourante);
            _context.Tuteurs.Add(tuteur);
            _context.Eleves.AddRange(eleveClasse10, eleveClasse20);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 20, 100));
            _context.V_Eleves.AddRange(
                new V_Eleve
                {
                    IdEleve = 1,
                    NomComplet = "Classe10 Test",
                    IdClasse = 10,
                    NomClasse = "6e A",
                    IdEcole = 1,
                    Statut = true,
                    DateNaissance = new DateTime(2015, 1, 1),
                    Nationalite = "RDC",
                    DateCreation = DateTime.Now
                },
                new V_Eleve
                {
                    IdEleve = 2,
                    NomComplet = "Classe20 Test",
                    IdClasse = 20,
                    NomClasse = "1ère B",
                    IdEcole = 1,
                    Statut = true,
                    DateNaissance = new DateTime(2016, 1, 1),
                    Nationalite = "RDC",
                    DateCreation = DateTime.Now
                });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllPagedAsync_WithoutFilters_ReturnsAllCurrentYearEleves()
        {
            var result = await _service.GetAllPagedAsync(1, new PagedRequest { PageNumber = 1, PageSize = 20 });

            result.Data.Data.Should().HaveCount(2);
            result.Data.Data.Select(e => e.IdEleve).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task GetAllPagedAsync_WithIdClasse_ReturnsOnlyThatClass()
        {
            var result = await _service.GetAllPagedAsync(
                1, new PagedRequest { PageNumber = 1, PageSize = 20 }, idClasse: 10);

            result.Data.Data.Should().ContainSingle(e => e.IdEleve == 1);
            result.Data.Data.Should().NotContain(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task GetAllPagedAsync_WithIdDirection_ReturnsAllClassesInDirection()
        {
            var result = await _service.GetAllPagedAsync(
                1, new PagedRequest { PageNumber = 1, PageSize = 20 }, idDirection: 2);

            result.Data.Data.Should().ContainSingle(e => e.IdEleve == 2);
            result.Data.Data.Should().NotContain(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task GetAllPagedAsync_WithIdClasseAndMatchingDirection_UsesClasseFilter()
        {
            var result = await _service.GetAllPagedAsync(
                1, new PagedRequest { PageNumber = 1, PageSize = 20 }, idClasse: 10, idDirection: 1);

            result.Data.Data.Should().ContainSingle(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task GetAllPagedAsync_WithIdClasseAndWrongDirection_Throws()
        {
            var act = async () => await _service.GetAllPagedAsync(
                1, new PagedRequest { PageNumber = 1, PageSize = 20 }, idClasse: 10, idDirection: 2);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*n'appartient pas à la direction*");
        }

        [Fact]
        public async Task GetAllPagedAsync_WithClasseFromOtherEcole_Throws()
        {
            var otherEcole = TestDataBuilder.CreateEcole(99, "Autre");
            var otherDir = TestDataBuilder.CreateDirection(99, 99);
            var otherClasse = TestDataBuilder.CreateClasse(99, "Autre", idDirection: 99);
            _context.Ecoles.Add(otherEcole);
            _context.Directions.Add(otherDir);
            _context.Classes.Add(otherClasse);
            await _context.SaveChangesAsync();

            var act = async () => await _service.GetAllPagedAsync(
                1, new PagedRequest { PageNumber = 1, PageSize = 20 }, idClasse: 99);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*n'appartient pas à l'école*");
        }

        [Fact]
        public async Task GetAllPagedAsync_OverlaysClasseFromRequestedYear_NotLatestInscription()
        {
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                99, 1, "Precedente",
                debut: _now.AddYears(-1),
                fin: _now.AddMonths(-4)));

            var inscriptionAnneeCourante = _context.Inscriptions.Single(i => i.IdInscription == 1);
            inscriptionAnneeCourante.DateInscription = _now.AddMonths(-2);

            // Inscription plus récente (N-1 / autre classe) → ce que V_Eleve exposerait comme "latest"
            _context.Inscriptions.Add(
                TestDataBuilder.CreateInscription(3, 1, 1, 20, 99, dateInscription: _now));

            var vue = _context.V_Eleves.Single(v => v.IdEleve == 1);
            vue.IdClasse = 20;
            vue.NomClasse = "1ère B";
            await _context.SaveChangesAsync();

            var result = await _service.GetAllPagedAsync(
                1, new PagedRequest { PageNumber = 1, PageSize = 20 }, idAnneeScolaire: 100);

            var eleve = result.Data.Data.Should().ContainSingle(e => e.IdEleve == 1).Subject;
            eleve.IdClasse.Should().Be(10);
            eleve.NomClasse.Should().Be("6e A");
        }

        [Fact]
        public async Task GetAllCursorPagedAsync_OverlaysClasseFromRequestedYear()
        {
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                99, 1, "Precedente",
                debut: _now.AddYears(-1),
                fin: _now.AddMonths(-4)));
            _context.Inscriptions.Single(i => i.IdInscription == 1).DateInscription = _now.AddMonths(-2);
            _context.Inscriptions.Add(
                TestDataBuilder.CreateInscription(3, 1, 1, 20, 99, dateInscription: _now));
            var vue = _context.V_Eleves.Single(v => v.IdEleve == 1);
            vue.IdClasse = 20;
            vue.NomClasse = "1ère B";
            await _context.SaveChangesAsync();

            var result = await _service.GetAllCursorPagedAsync(
                1, new CursorPaginationRequest { Limit = 20 }, idAnneeScolaire: 100);

            var eleve = result.Data.Data.Should().ContainSingle(e => e.IdEleve == 1).Subject;
            eleve.IdClasse.Should().Be(10);
            eleve.NomClasse.Should().Be("6e A");
        }

        public void Dispose() => _context.Dispose();
    }
}
