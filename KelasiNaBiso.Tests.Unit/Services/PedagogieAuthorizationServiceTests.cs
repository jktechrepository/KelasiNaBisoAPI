using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PedagogieAuthorizationServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly PedagogieAuthorizationService _service;
        private readonly DateTime _now = DateTime.Now;

        public PedagogieAuthorizationServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _service = new PedagogieAuthorizationService(
                _context,
                new AnneeScolaireService(_context),
                NullLogger<PedagogieAuthorizationService>.Instance);
            Seed();
        }

        private void Seed()
        {
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole");
            var direction = TestDataBuilder.CreateDirection(1, 1);
            var classe = TestDataBuilder.CreateClasse(10, "6e", idDirection: 1);
            var annee = TestDataBuilder.CreateAnneeScolaire(100, 1, "Courante", _now.AddMonths(-2), _now.AddMonths(6));
            var anneePast = TestDataBuilder.CreateAnneeScolaire(99, 1, "Past", _now.AddYears(-2), _now.AddYears(-1));
            var agentTitulaire = TestDataBuilder.CreateAgent(1, "Titulaire", idEcole: 1);
            var agentAffecte = TestDataBuilder.CreateAgent(2, "Affecte", idEcole: 1);
            var cours = new Models.Cours
            {
                IdCours = 50,
                NomCours = "Math",
                IdClasse = 10,
                Statut = true,
                DateCreation = DateTime.Now
            };

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Classes.Add(classe);
            _context.AnneeScolaires.AddRange(annee, anneePast);
            _context.Agents.AddRange(agentTitulaire, agentAffecte);
            _context.Cours.Add(cours);
            _context.TitulairesClasses.Add(new Models.TitulaireClasse
            {
                IdTitulaireClasse = 1,
                IdAgent = 1,
                IdClasse = 10,
                IdAnneeScolaire = 100,
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.AffectationsCours.Add(new Models.AffectationCours
            {
                IdAffectationCours = 1,
                IdAgent = 2,
                IdCours = 50,
                IdAnneeScolaire = 100,
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task AgentEnseigneClasse_Titulaire_CurrentYear_ReturnsTrue()
        {
            var ok = await _service.AgentEnseigneClasseAsync(1, 10, null);
            ok.Should().BeTrue();
        }

        [Fact]
        public async Task AgentEnseigneClasse_Affectation_CurrentYear_ReturnsTrue()
        {
            var ok = await _service.AgentEnseigneClasseAsync(2, 10, null);
            ok.Should().BeTrue();
        }

        [Fact]
        public async Task AgentEnseigneClasse_WrongYear_ReturnsFalse()
        {
            var ok = await _service.AgentEnseigneClasseAsync(1, 10, 99);
            ok.Should().BeFalse();
        }

        [Fact]
        public async Task GetClassesEnseignant_MergesTitulaireAndAffectation()
        {
            var classes = await _service.GetClassesEnseignantAsync(1, idEcole: 1);
            classes.Should().Contain(10);

            var classes2 = await _service.GetClassesEnseignantAsync(2, idEcole: 1);
            classes2.Should().Contain(10);
        }

        public void Dispose() => _context.Dispose();
    }
}
