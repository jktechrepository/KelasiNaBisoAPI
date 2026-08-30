using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class AnneeScolaireCouranteTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly AnneeScolaireService _service;
        private readonly DateTime _now = DateTime.Now;

        public AnneeScolaireCouranteTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _service = new AnneeScolaireService(_context);
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAnneeCourante_EnSession_RetourneAnneeEnCours()
        {
            var anneeA = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6));
            _context.AnneeScolaires.Add(anneeA);
            await _context.SaveChangesAsync();

            var result = await _service.GetAnneeCouranteAsync(1);

            result.Should().NotBeNull();
            result!.IdAnneeScolaire.Should().Be(100);
        }

        [Fact]
        public async Task GetAnneeCourante_VacancesEntreAetB_RetourneAnneeB()
        {
            var finA = _now.AddDays(-30);
            var debutB = _now.AddDays(30);

            var anneeA = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: finA.AddMonths(-10),
                fin: finA);
            var anneeB = TestDataBuilder.CreateAnneeScolaire(
                101, 1, "2025-2026",
                debut: debutB,
                fin: debutB.AddMonths(10));

            _context.AnneeScolaires.AddRange(anneeA, anneeB);
            await _context.SaveChangesAsync();

            var result = await _service.GetAnneeCouranteAsync(1);

            result.Should().NotBeNull();
            result!.IdAnneeScolaire.Should().Be(101);
        }

        [Fact]
        public async Task ResolveIdAnneeScolaire_Explicite_BypassVacances()
        {
            var finA = _now.AddDays(-30);
            var debutB = _now.AddDays(30);

            var anneeA = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: finA.AddMonths(-10),
                fin: finA);
            var anneeB = TestDataBuilder.CreateAnneeScolaire(
                101, 1, "2025-2026",
                debut: debutB,
                fin: debutB.AddMonths(10));

            _context.AnneeScolaires.AddRange(anneeA, anneeB);
            await _context.SaveChangesAsync();

            var scope = new EleveAnneeScopeHelper(
                _context,
                new InscriptionActiveResolver(_context),
                _service);

            var id = await scope.ResolveIdAnneeScolaireAsync(1, 100);

            id.Should().Be(100);
        }

        [Fact]
        public async Task GetAnneeCourante_VacancesSansAnneeB_RetourneNull()
        {
            var finA = _now.AddDays(-30);
            var anneeA = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: finA.AddMonths(-10),
                fin: finA);

            _context.AnneeScolaires.Add(anneeA);
            await _context.SaveChangesAsync();

            var result = await _service.GetAnneeCouranteAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAnneeCourante_AucuneAnnee_RetourneNull()
        {
            var result = await _service.GetAnneeCouranteAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAnneeCourante_VacancesAnneeBInactive_RetourneNull()
        {
            var finA = _now.AddDays(-30);
            var debutB = _now.AddDays(30);

            var anneeA = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: finA.AddMonths(-10),
                fin: finA);
            var anneeB = TestDataBuilder.CreateAnneeScolaire(
                101, 1, "2025-2026",
                debut: debutB,
                fin: debutB.AddMonths(10),
                statut: false);

            _context.AnneeScolaires.AddRange(anneeA, anneeB);
            await _context.SaveChangesAsync();

            var result = await _service.GetAnneeCouranteAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAnneePrecedente_EnSession_RetourneAnneeAvantCourante()
        {
            var anneePast = TestDataBuilder.CreateAnneeScolaire(
                99, 1, "2023-2024",
                debut: _now.AddYears(-2),
                fin: _now.AddYears(-1));
            var anneeCourante = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6));

            _context.AnneeScolaires.AddRange(anneePast, anneeCourante);
            await _context.SaveChangesAsync();

            var result = await _service.GetAnneePrecedenteAsync(1);

            result.Should().NotBeNull();
            result!.IdAnneeScolaire.Should().Be(99);
        }

        public void Dispose() => _context.Dispose();
    }
}
