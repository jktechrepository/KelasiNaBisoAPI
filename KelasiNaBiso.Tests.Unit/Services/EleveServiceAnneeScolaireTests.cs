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
    public class EleveServiceAnneeScolaireTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _service;
        private readonly DateTime _now = DateTime.Now;

        public EleveServiceAnneeScolaireTests()
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
            var direction = TestDataBuilder.CreateDirection(1, 1);
            var classe = TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1);

            // Année courante (couvre aujourd'hui)
            var anneeCourante = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "Courante",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6));

            // Année antérieure
            var anneePast = TestDataBuilder.CreateAnneeScolaire(
                99, 1, "Precedente",
                debut: _now.AddYears(-2),
                fin: _now.AddYears(-1));

            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent");
            var eleveCourant = TestDataBuilder.CreateEleve(1, 1, "Courant");
            var elevePast = TestDataBuilder.CreateEleve(2, 1, "Ancien");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Classes.Add(classe);
            _context.AnneeScolaires.AddRange(anneeCourante, anneePast);
            _context.Tuteurs.Add(tuteur);
            _context.Eleves.AddRange(eleveCourant, elevePast);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 99));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByEcoleAsync_WithoutAnnee_ReturnsOnlyCurrentYearEleves()
        {
            var result = await _service.GetByEcoleAsync(1, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.IdEcole.Should().Be(1);
            result.Data.Should().ContainSingle(e => e.IdEleve == 1);
            result.Data.Should().NotContain(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task GetByEcoleAsync_WithExplicitPastAnnee_ReturnsOnlyThatYear()
        {
            var result = await _service.GetByEcoleAsync(1, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Should().ContainSingle(e => e.IdEleve == 2);
            result.Data.Should().NotContain(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task GetByEcoleAsync_WithoutCurrentYear_Throws()
        {
            foreach (var annee in _context.AnneeScolaires)
            {
                annee.DateDebut = _now.AddYears(-5);
                annee.DateFin = _now.AddYears(-4);
            }
            await _context.SaveChangesAsync();

            var act = async () => await _service.GetByEcoleAsync(1, null);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Aucune année scolaire en cours*");
        }

        [Fact]
        public async Task GetByClasseAsync_DefaultsToCurrentYear()
        {
            var result = await _service.GetByClasseAsync(10, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.Data.Should().ContainSingle(e => e.IdEleve == 1);
            result.Data.Should().NotContain(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task GetByClasseAsync_WithPastAnnee_ReturnsPastEleve()
        {
            var result = await _service.GetByClasseAsync(10, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Should().ContainSingle(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task GetByTuteurAsync_DefaultYear_ReturnsStudentAndYearFields()
        {
            var result = await _service.GetByTuteurAsync(1);

            result.Should().ContainSingle(e => e.IdEleve == 1);

            var eleve = result.Single();
            eleve.IdEleve.Should().Be(1);
            eleve.IdAnneeScolaire.Should().Be(100);
            eleve.LibelleAnneeScolaire.Should().Be("Courante");
        }

        [Fact]
        public async Task GetByTuteurAsync_LibelleAnneeScolaire_ReturnsMatchingYear()
        {
            var result = await _service.GetByTuteurAsync(
                1, libelleAnneeScolaire: "Precedente");

            result.Should().ContainSingle(e => e.IdEleve == 2);
            result.Single().IdAnneeScolaire.Should().Be(99);
            result.Single().LibelleAnneeScolaire.Should().Be("Precedente");
        }

        [Fact]
        public async Task GetByTuteurAsync_IdEleve_FiltersWithinTuteurAndYear()
        {
            var result = await _service.GetByTuteurAsync(1, idEleve: 1);

            result.Should().ContainSingle(e => e.IdEleve == 1);
            result.Single().IdAnneeScolaire.Should().Be(100);

            var otherYear = await _service.GetByTuteurAsync(1, idEleve: 2);

            otherYear.Should().BeEmpty();
        }

        public void Dispose() => _context.Dispose();
    }
}
