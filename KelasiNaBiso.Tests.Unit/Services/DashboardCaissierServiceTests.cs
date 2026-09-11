using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Tests.Unit.Helpers;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DashboardCaissierServiceTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly DashboardCaissierService _service;
        private readonly DateTime _today = DateTime.Now.Date;

        public DashboardCaissierServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, anneeRepo);

            var mokoMock = new Mock<IEcolePaiementMobileService>();
            mokoMock.Setup(m => m.GetOverviewAsync(It.IsAny<int>()))
                .ReturnsAsync(new EcolePaiementMobileOverviewDto { IdEcole = 1, EstConfigure = true });

            _service = new DashboardCaissierService(_context, scope, mokoMock.Object);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Caissier"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026", _today.AddMonths(-3), _today.AddMonths(6)));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 101));
            _context.Frais.Add(new Frais
            {
                IdFrais = 1,
                LibelleFrais = "Minerval",
                Montant = 100,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 101,
                Portee = KelasiNaBiso.Models.Enums.PorteeFrais.Direction,
                Statut = true,
                DateCreation = DateTime.Now
            });

            _context.Paiements.AddRange(
                new Paiement
                {
                    IdPaiement = 1,
                    IdEleve = 1,
                    IdFrais = 1,
                    IdUtilisateur = 10,
                    Montant = 50,
                    ModePaiement = "Cash",
                    StatutPaiement = "Confirme",
                    Statut = true,
                    DatePaiement = _today.AddHours(9)
                },
                new Paiement
                {
                    IdPaiement = 2,
                    IdEleve = 1,
                    IdFrais = 1,
                    IdUtilisateur = 20,
                    Montant = 80,
                    ModePaiement = "Cash",
                    StatutPaiement = "Confirme",
                    Statut = true,
                    DatePaiement = _today.AddHours(10)
                },
                new Paiement
                {
                    IdPaiement = 3,
                    IdEleve = 1,
                    IdFrais = 1,
                    IdUtilisateur = 10,
                    Montant = 30,
                    ModePaiement = "Mobile Money",
                    StatutPaiement = "En attente",
                    Statut = true,
                    DatePaiement = _today.AddHours(11),
                    OperateurMobileMoney = "airtel"
                });

            _context.TransactionsMoko.AddRange(
                new TransactionMoko
                {
                    IdTransactionMoko = 1,
                    Reference = "MOKO-PENDING-1",
                    IdPaiement = 3,
                    IdEcole = 1,
                    Action = MokoActions.Debit,
                    Amount = 30,
                    Status = MokoTransactionStatuses.Pending,
                    DateCreation = _today.AddHours(11)
                },
                new TransactionMoko
                {
                    IdTransactionMoko = 2,
                    Reference = "MOKO-OK-1",
                    IdPaiement = 1,
                    IdEcole = 1,
                    Action = MokoActions.Debit,
                    Amount = 50,
                    Status = MokoTransactionStatuses.Success,
                    DateCreation = _today.AddHours(9)
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetDashboardCaissierAsync_ShouldReturnOnlyCurrentUserPaymentsForToday()
        {
            var result = await _service.GetDashboardCaissierAsync(1, 10, 101, _today);

            result.Resume.NombrePaiements.Should().Be(2);
            result.Resume.MontantTotal.Should().Be(80);
            result.DerniersPaiements.Should().HaveCount(2);
            result.DerniersPaiements.Select(p => p.IdPaiement).Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Fact]
        public async Task GetDashboardCaissierAsync_ShouldExcludeOtherCashierPayments()
        {
            var result = await _service.GetDashboardCaissierAsync(1, 20, 101, _today);

            result.Resume.NombrePaiements.Should().Be(1);
            result.Resume.MontantTotal.Should().Be(80);
        }

        [Fact]
        public async Task GetDashboardCaissierAsync_ShouldAggregateModesAndMokoPending()
        {
            var result = await _service.GetDashboardCaissierAsync(1, 10, 101, _today);

            result.RepartitionParMode.Modes.Should().ContainKey("Cash");
            result.RepartitionParMode.Modes.Should().ContainKey("Mobile Money");
            result.Moko.EstConfigure.Should().BeTrue();
            result.Moko.MesPayInsEnAttente.Should().Be(1);
            result.Moko.PayInsEnAttente.Single().Reference.Should().Be("MOKO-PENDING-1");
            result.Resume.PayInsReussis.Should().Be(1);
            result.Resume.PayInsEnAttente.Should().Be(1);
            result.Scope.Should().Be(DashboardCaissierScopes.Moi);
        }

        [Fact]
        public async Task GetDashboardCaissierAsync_ShouldReturnAllSchoolPayments_WhenScopeEcoleAllowed()
        {
            var result = await _service.GetDashboardCaissierAsync(
                1, 10, 101, _today, DashboardCaissierScopes.Ecole, allowEcoleScope: true);

            result.Scope.Should().Be(DashboardCaissierScopes.Ecole);
            result.Resume.NombrePaiements.Should().Be(3);
            result.Resume.MontantTotal.Should().Be(160);
        }

        [Fact]
        public async Task GetDashboardCaissierAsync_ShouldIgnoreEcoleScope_WhenNotAllowed()
        {
            var result = await _service.GetDashboardCaissierAsync(
                1, 10, 101, _today, DashboardCaissierScopes.Ecole, allowEcoleScope: false);

            result.Scope.Should().Be(DashboardCaissierScopes.Moi);
            result.Resume.NombrePaiements.Should().Be(2);
        }

        [Fact]
        public async Task GetClotureCaissierAsync_ShouldReturnAllPaymentsOfDay()
        {
            var result = await _service.GetClotureCaissierAsync(1, 10, 101, _today);

            result.TousLesPaiements.Should().HaveCount(2);
            result.DerniersPaiements.Should().HaveCount(2);
            result.GenereLe.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
