using FluentAssertions;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveReinscriptionPrefillTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _service;
        private readonly DateTime _now = DateTime.Now;

        public EleveReinscriptionPrefillTests()
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

            var anneeCourante = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "Courante",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6));

            var anneePast = TestDataBuilder.CreateAnneeScolaire(
                99, 1, "Precedente",
                debut: _now.AddYears(-2),
                fin: _now.AddYears(-1));

            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent Kabila", email: "parent@test.com");
            var eleveCourant = TestDataBuilder.CreateEleve(1, 1, "Courant");
            eleveCourant.Matricule = "MAT-COURANT-001";
            eleveCourant.LieuNaissance = "Kinshasa";
            eleveCourant.Province = "Kinshasa";

            var elevePast = TestDataBuilder.CreateEleve(2, 1, "Ancien");
            elevePast.Matricule = "MAT-ANCIEN-002";
            elevePast.NomComplet = "Ancien Test Jean";

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
        public async Task GetReinscriptionByMatricule_DefaultYear_ReturnsPastYearStudent()
        {
            var result = await _service.GetReinscriptionPrefillByMatriculeAsync(1, "MAT-ANCIEN-002");

            result.Should().NotBeNull();
            result!.IdEleveExistant.Should().Be(2);
            result.IdTuteurExistant.Should().Be(1);
            result.Type.Should().Be("Réinscription");
            result.IdAnneeScolaireReference.Should().Be(99);
            result.LibelleAnneeScolaireReference.Should().Be("Precedente");
            result.IdClassePrecedente.Should().Be(10);
            result.NomCompletTuteur.Should().Be("Parent Kabila");
            result.DejaInscritAnneeCourante.Should().BeFalse();
        }

        [Fact]
        public async Task GetReinscriptionByMatricule_CurrentYearStudent_FlagsDejaInscrit()
        {
            var result = await _service.GetReinscriptionPrefillByMatriculeAsync(1, "MAT-COURANT-001", idAnneeScolaire: 100);

            result.Should().NotBeNull();
            result!.IdEleveExistant.Should().Be(1);
            result.DejaInscritAnneeCourante.Should().BeTrue();
            result.IdInscriptionAnneeCourante.Should().Be(1);
        }

        [Fact]
        public async Task GetReinscriptionByMatricule_UnknownMatricule_ReturnsNull()
        {
            var result = await _service.GetReinscriptionPrefillByMatriculeAsync(1, "INEXISTANT");

            result.Should().BeNull();
        }

        [Fact]
        public async Task SearchReinscriptionByNomComplet_DefaultYear_ReturnsPastStudent()
        {
            var result = await _service.SearchReinscriptionPrefillByNomCompletPagedAsync(
                1,
                "Ancien",
                new PagedRequest { PageNumber = 1, PageSize = 10 });

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Data.Should().ContainSingle(e => e.IdEleveExistant == 2);
            result.Data.Data.Should().NotContain(e => e.IdEleveExistant == 1);
        }

        [Fact]
        public async Task SearchReinscriptionByNomComplet_WithClasseFilter_ScopesResults()
        {
            var result = await _service.SearchReinscriptionPrefillByNomCompletPagedAsync(
                1,
                "Test",
                new PagedRequest { PageNumber = 1, PageSize = 10 },
                idAnneeScolaire: 99,
                idClasse: 10);

            result.Data.Data.Should().ContainSingle(e => e.IdEleveExistant == 2);
        }

        [Fact]
        public async Task SearchReinscriptionByNomComplet_Pagination_RespectsPageSize()
        {
            var result = await _service.SearchReinscriptionPrefillByNomCompletPagedAsync(
                1,
                "Test",
                new PagedRequest { PageNumber = 1, PageSize = 1 },
                idAnneeScolaire: 99);

            result.Data.Data.Should().HaveCount(1);
            result.Data.TotalRecords.Should().Be(1);
        }

        public void Dispose() => _context.Dispose();
    }
}
