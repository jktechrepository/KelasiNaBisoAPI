using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class TuteurAnneeScolaireTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly TuteurService _service;
        private readonly DateTime _now = DateTime.Now;

        public TuteurAnneeScolaireTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new TuteurService(_context, inscriptionResolver, scope);
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

            var tuteurCourant = TestDataBuilder.CreateTuteur(1, "Parent Courant");
            var tuteurPast = TestDataBuilder.CreateTuteur(2, "Parent Ancien");
            var eleveCourant = TestDataBuilder.CreateEleve(1, 1, "Courant");
            var elevePast = TestDataBuilder.CreateEleve(2, 2, "Ancien");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Classes.Add(classe);
            _context.AnneeScolaires.AddRange(anneeCourante, anneePast);
            _context.Tuteurs.AddRange(tuteurCourant, tuteurPast);
            _context.Eleves.AddRange(eleveCourant, elevePast);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 99));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByEcoleAsync_WithoutAnnee_ReturnsOnlyCurrentYearTuteurs()
        {
            var result = await _service.GetByEcoleAsync(1, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.Data.Should().ContainSingle(t => t.IdTuteur == 1);
            result.Data.Should().NotContain(t => t.IdTuteur == 2);
        }

        [Fact]
        public async Task GetByEcoleAsync_WithPastAnnee_ReturnsOnlyPastYearTuteurs()
        {
            var result = await _service.GetByEcoleAsync(1, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Should().ContainSingle(t => t.IdTuteur == 2);
            result.Data.Should().NotContain(t => t.IdTuteur == 1);
        }

        [Fact]
        public async Task GetElevesAsync_WithoutAnnee_ReturnsOnlyCurrentYearChildren()
        {
            var result = await _service.GetElevesAsync(1);

            result.Should().ContainSingle(e => e.IdEleve == 1);
            result.Should().NotContain(e => e.IdEleve == 2);
            result.Single().IdAnneeScolaire.Should().Be(100);
            result.Single().LibelleAnneeScolaire.Should().Be("Courante");
        }

        [Fact]
        public async Task GetElevesAsync_WithPastAnnee_ReturnsOnlyPastYearChildren()
        {
            var result = await _service.GetElevesAsync(2, libelleAnneeScolaire: "Precedente");

            result.Should().ContainSingle(e => e.IdEleve == 2);
            result.Single().IdAnneeScolaire.Should().Be(99);
            result.Single().LibelleAnneeScolaire.Should().Be("Precedente");
        }

        [Fact]
        public async Task GetElevesAsync_ShouldIncludeEcoleAndClasseFields()
        {
            var result = await _service.GetElevesAsync(1);

            var eleve = result.Should().ContainSingle().Subject;
            eleve.IdEcole.Should().Be(1);
            eleve.NomEcole.Should().Be("Ecole Test");
            eleve.IdClasse.Should().Be(10);
            eleve.NomClasse.Should().Be("6e A");
            eleve.IdAnneeScolaire.Should().Be(100);
            eleve.LibelleAnneeScolaire.Should().Be("Courante");
        }

        [Fact]
        public async Task GetElevesAsync_WithLibelleAnneeScolaire_ReturnsMatchingYear()
        {
            var result = await _service.GetElevesAsync(
                2, libelleAnneeScolaire: "Precedente");

            result.Should().ContainSingle(e => e.IdEleve == 2);
            result.Single().IdAnneeScolaire.Should().Be(99);
            result.Single().LibelleAnneeScolaire.Should().Be("Precedente");
        }

        [Fact]
        public async Task GetElevesAsync_ShouldFilterBySearchTerm()
        {
            var result = await _service.GetElevesAsync(1, searchTerm: "Courant");

            result.Should().ContainSingle(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task GetElevesAsync_SearchTermNoMatch_ReturnsEmpty()
        {
            var result = await _service.GetElevesAsync(1, searchTerm: "Inexistant");

            result.Should().BeEmpty();
        }

        public void Dispose() => _context.Dispose();
    }
}
