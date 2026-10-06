using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Tests.Unit.Helpers;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DashboardFinancierServiceTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly DashboardFinancierService _service;
        private readonly DateTime _date = DateTime.Today;

        public DashboardFinancierServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(
                _context,
                resolver,
                new AnneeScolaireService(_context));

            var moko = new Mock<IEcolePaiementMobileService>();
            moko.Setup(m => m.GetOverviewAsync(It.IsAny<int>()))
                .ReturnsAsync(new Models.DTOs.MokoAfrika.EcolePaiementMobileOverviewDto());

            _service = new DashboardFinancierService(
                _context,
                scope,
                moko.Object,
                new CurrencyConversionService(_context));

            Seed();
        }

        [Fact]
        public async Task GetDashboardFinancierAsync_ShouldAggregatePaymentsInSchoolPrincipalCurrency()
        {
            var result = await _service.GetDashboardFinancierAsync(1, 10, 101, _date);

            result.Ecole.DevisePrincipale.Should().Be("CDF");
            result.Resume.MontantTotal.Should().Be(145000m);
            result.Resume.MontantConfirme.Should().Be(140000m);
            result.Resume.MontantEnAttente.Should().Be(5000m);
            result.RepartitionParMode.Modes["Cash"].Montant.Should().Be(140000m);
            result.DerniersPaiements.Should().ContainSingle(p =>
                p.CodeDevisePaiement == "USD"
                && p.Montant == 50m
                && p.MontantDevisePrincipale == 140000m);
        }

        private void Seed()
        {
            _context.Ecoles.Add(new Ecole
            {
                IdEcole = 1,
                Nom = "Ecole Finance",
                CodeDevisePrincipale = "CDF",
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026", _date.AddMonths(-3), _date.AddMonths(6)));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 101));
            _context.Frais.Add(TestDataBuilder.CreateFrais(1, 1, 101, "Minerval", montant: 100));
            _context.DevisesMonetaires.AddRange(
                new DeviseMonetaire { IdEcole = 1, CodeDevise = "CDF", Libelle = "Franc congolais", Statut = true },
                new DeviseMonetaire { IdEcole = 1, CodeDevise = "USD", Libelle = "Dollar américain", Statut = true });
            _context.TauxChanges.Add(new TauxChange
            {
                IdEcole = 1,
                CodeDeviseSource = "USD",
                CodeDeviseCible = "CDF",
                Taux = 2800m,
                DateEffet = _date.AddDays(-1),
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
                    DatePaiement = _date.AddHours(9),
                    Montant = 50,
                    Devise = "USD",
                    CodeDevisePaiement = "USD",
                    StatutPaiement = "Confirme",
                    Statut = true,
                    ModePaiement = "Cash"
                },
                new Paiement
                {
                    IdPaiement = 2,
                    IdEleve = 1,
                    IdFrais = 1,
                    IdUtilisateur = 10,
                    DatePaiement = _date.AddHours(10),
                    Montant = 5000,
                    Devise = "CDF",
                    CodeDevisePaiement = "CDF",
                    StatutPaiement = "En attente",
                    Statut = true,
                    ModePaiement = "Mobile Money"
                });
            _context.SaveChanges();
        }

        public void Dispose() => _context.Dispose();
    }
}
