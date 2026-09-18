using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DashboardEleveServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DashboardEleveService _service;
        private readonly DateTime _now = DateTime.Now;

        public DashboardEleveServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new DashboardEleveService(_context, scope);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Alpha"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(11, "Parent"));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6)));

            var eleve = TestDataBuilder.CreateEleve(1, 11, "Alice");
            eleve.Matricule = "MAT-DASH-01";
            _context.Eleves.Add(eleve);
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 101));
            _context.Frais.Add(TestDataBuilder.CreateFrais(2, 1, 101, "Frais N", montant: 120));
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 2,
                IdEleve = 1,
                IdFrais = 2,
                Montant = 40,
                Devise = "USD",
                DatePaiement = _now.AddDays(-5),
                Statut = true,
                StatutPaiement = "Confirme",
                ReferencePaiemenet = "CUR"
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task BuildAsync_WithInscriptionAndPaiements_ReturnsProfilAndResume()
        {
            var dto = await _service.BuildAsync(1, 1, null);

            dto.Profil.Should().NotBeNull();
            dto.Profil!.IdEleve.Should().Be(1);
            dto.Profil.Matricule.Should().Be("MAT-DASH-01");
            dto.Profil.IdClasse.Should().Be(10);
            dto.Resume.NombrePaiements.Should().Be(1);
            dto.Resume.MontantPaye.Should().Be(40);
            dto.Resume.AlertePaiement.Should().BeFalse();
            dto.Ecole!.IdEcole.Should().Be(1);
        }

        [Fact]
        public async Task BuildAsync_WithoutInscription_ReturnsEmptyProfilAndInfoAlerte()
        {
            var dto = await _service.BuildAsync(999, 1, null);

            dto.Profil.Should().BeNull();
            dto.Alertes.Should().Contain(a => a.Type == "info");
            dto.Resume.MessagePaiement.Should().Contain("Aucune inscription");
        }

        public void Dispose() => _context.Dispose();
    }
}
