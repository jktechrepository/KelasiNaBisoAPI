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
    public class DevoirTelechargementTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DevoirADomicileService _service;

        public DevoirTelechargementTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
            _service = new DevoirADomicileService(
                _context,
                Mock.Of<ICurrentUserService>(c => c.UserId == 42),
                NullLogger<DevoirADomicileService>.Instance,
                inscriptionResolver,
                scope);

            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e", idDirection: 1));
            _context.Agents.Add(TestDataBuilder.CreateAgent(1, "Prof", idEcole: 1));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(6)));
            _context.Utilisateurs.Add(TestDataBuilder.CreateUtilisateur(42, "eleve@test.com", "Eleve", idEcole: 1));
            _context.DevoirsADomicile.Add(new DevoirADomicile
            {
                IdDevoirADomicile = 1,
                Titre = "Devoir PDF",
                IdEcole = 1,
                IdDirection = 1,
                IdAgent = 1,
                IdClasse = 10,
                IdAnneeScolaire = 101,
                NombreTelechargements = 0,
                Statut = true,
                NomFichier = "d.pdf",
                CheminFichier = "devoirs/d.pdf"
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task EnregistrerTelechargement_Premier_CreeSuivi_EtIncrementeGlobal()
        {
            var ok = await _service.EnregistrerTelechargementAsync(1, 42);
            ok.Should().BeTrue();

            var devoir = await _context.DevoirsADomicile.FindAsync(1);
            devoir!.NombreTelechargements.Should().Be(1);

            _context.DevoirsADomicileTelechargements.Should().ContainSingle(t =>
                t.IdDevoirADomicile == 1 && t.IdUtilisateur == 42);

            var ids = await _service.GetDevoirIdsTelechargesParUtilisateurAsync(42, new[] { 1 });
            ids.Should().Contain(1);
        }

        [Fact]
        public async Task EnregistrerTelechargement_Second_PasDeDoublon_GlobalPlusUn()
        {
            await _service.EnregistrerTelechargementAsync(1, 42);
            await _service.EnregistrerTelechargementAsync(1, 42);

            var devoir = await _context.DevoirsADomicile.FindAsync(1);
            devoir!.NombreTelechargements.Should().Be(2);

            _context.DevoirsADomicileTelechargements.Count(t =>
                t.IdDevoirADomicile == 1 && t.IdUtilisateur == 42).Should().Be(1);
        }

        [Fact]
        public async Task GetDevoirIdsTelecharges_SansHistorique_RetourneVide()
        {
            var ids = await _service.GetDevoirIdsTelechargesParUtilisateurAsync(42, new[] { 1 });
            ids.Should().BeEmpty();
        }

        public void Dispose() => _context.Dispose();
    }
}
