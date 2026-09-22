using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveRegistreEleveTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _sut;
        private readonly DateTime _now = DateTime.Now;

        public EleveRegistreEleveTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _sut = new EleveService(
                _context,
                Mock.Of<IInscriptionRepository>(),
                resolver,
                scope,
                NullLogger<EleveService>.Instance);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Alpha"));
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(2, "Ecole Beta"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Directions.Add(TestDataBuilder.CreateDirection(2, 2));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(20, "5e B", idDirection: 2));

            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: _now.AddYears(-1),
                fin: _now.AddMonths(-3)));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                101, 1, "2025-2026",
                debut: _now.AddMonths(-2),
                fin: _now.AddMonths(10)));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                200, 2, "2025-2026",
                debut: _now.AddMonths(-2),
                fin: _now.AddMonths(10)));

            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(1, "Parent"));

            // Élève cible — 2 inscriptions confirmées → dernière année 2025-2026 / 6e A
            var mukendi = TestDataBuilder.CreateEleve(1, 1, "Mukendi");
            mukendi.Matricule = "MAT-SECRET-001";
            _context.Eleves.Add(mukendi);
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                1, 1, 1, 10, 100, dateInscription: _now.AddYears(-1)));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                2, 1, 1, 10, 101, dateInscription: _now.AddMonths(-1)));

            // Autre élève même préfixe de nom, école différente
            _context.Eleves.Add(TestDataBuilder.CreateEleve(2, 1, "Mukeba"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                3, 2, 2, 20, 200, dateInscription: _now.AddDays(-10)));

            // Inactif — exclu
            var inactif = TestDataBuilder.CreateEleve(3, 1, "Mukendi", statut: false);
            _context.Eleves.Add(inactif);
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                4, 3, 1, 10, 101));

            // Actif sans inscription confirmée — exclu du résultat
            var enAttente = TestDataBuilder.CreateEleve(4, 1, "Mukoko");
            _context.Eleves.Add(enAttente);
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                5, 4, 1, 10, 101, statutInscription: "En attente"));

            _context.SaveChanges();
        }

        [Fact]
        public async Task Registre_ReturnsMinimalFields_LatestConfirmedYear()
        {
            var items = await _sut.GetRegistreEleveAsync("Mukendi");

            items.Should().ContainSingle();
            var item = items[0];
            item.Nom.Should().Be("Mukendi");
            item.Postnom.Should().Be("Test");
            item.Prenom.Should().Be("Jean");
            item.NomClasse.Should().Be("6e A");
            item.NomEcole.Should().Be("Ecole Alpha");
            item.LibelleAnneeScolaire.Should().Be("2025-2026");
        }

        [Fact]
        public async Task Registre_FiltersByNomComplet_ExcludesInactiveAndPending()
        {
            var items = await _sut.GetRegistreEleveAsync("Muk");

            items.Select(i => i.Nom).Should().BeEquivalentTo(new[] { "Mukeba", "Mukendi" });
            items.Should().NotContain(i => i.Nom == "Mukoko");
        }

        [Fact]
        public async Task Registre_ShortTerm_ReturnsEmpty()
        {
            var items = await _sut.GetRegistreEleveAsync("Mu");
            items.Should().BeEmpty();
        }

        [Fact]
        public async Task Registre_RespectsLimit()
        {
            var items = await _sut.GetRegistreEleveAsync("Muk", limit: 1);
            items.Should().ContainSingle();
        }

        public void Dispose() => _context.Dispose();
    }
}
