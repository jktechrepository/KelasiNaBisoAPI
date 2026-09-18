using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class MokoPayInConfirmCreatesPaiementTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly MokoAfrikaService _service;

        public MokoPayInConfirmCreatesPaiementTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.EcolesInfoPaiementMobile.Add(new EcoleInfoPaiementMobile
            {
                IdEcole = 1,
                Devise = "CDF",
                MobileMoneyActif = true,
                PayoutAutomatique = false,
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            _context.Frais.Add(TestDataBuilder.CreateFrais(1, 1, 1, "Minerval"));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(1, 1, "2025-2026"));
            _context.TransactionsMoko.Add(new TransactionMoko
            {
                IdTransactionMoko = 1,
                Reference = "MOKO_CONFIRM_REF",
                IdEcole = 1,
                Action = MokoActions.Debit,
                Amount = 104.5m,
                AmountNet = 100m,
                Devise = "CDF",
                Method = "mpesa",
                Status = MokoTransactionStatuses.Pending,
                RawRequest = PayInRawRequestHelper.Serialize(
                    new PayInIntentSnapshot
                    {
                        IdEleve = 1,
                        IdFrais = 1,
                        MontantNet = 100m,
                        MontantCollecte = 104.5m,
                        MontantGatewayNet = 100m,
                        CodeDeviseFrais = "CDF",
                        CodeDevisePrincipale = "CDF",
                        CodeDevisePaiement = "CDF",
                        MontantPayeDevisePrincipale = 100m,
                        ModePaiement = "Mobile Money",
                        OperateurMobileMoney = "mpesa"
                    },
                    new Dictionary<string, object?>()),
                DateCreation = DateTime.Now
            });
            _context.SaveChanges();

            var walletMock = new Mock<IMokoWalletService>();
            walletMock
                .Setup(w => w.CrediterApresPayInAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((int idEcole, int idPaiement, int idTx, decimal montant, string reference, string? devise, CancellationToken _) =>
                    new WalletCreditResult(montant, devise ?? "CDF", montant, devise ?? "CDF", 1m));

            _service = new MokoAfrikaService(
                _context,
                Mock.Of<IMokoAfrikaGatewayClient>(),
                Mock.Of<IMokoFeeCalculator>(),
                walletMock.Object,
                Mock.Of<IEcolePaiementMobileService>(),
                Mock.Of<IPaiementRepository>(),
                Mock.Of<IDashboardHubService>(),
                Options.Create(new MokoSettings()),
                NullLogger<MokoAfrikaService>.Instance);
        }

        [Fact]
        public async Task ConfirmerPayIn_CreatesConfirmedPaiement_LinksTransaction()
        {
            var idPaiement = await _service.ConfirmerPayInEtNotifierAsync(
                "MOKO_CONFIRM_REF", "PD-OK");

            idPaiement.Should().BeGreaterThan(0);
            var paiement = await _context.Paiements.SingleAsync();
            paiement.StatutPaiement.Should().Be("Confirme");
            paiement.IdEleve.Should().Be(1);
            paiement.IdFrais.Should().Be(1);
            paiement.MontantNet.Should().Be(100m);
            paiement.Devise.Should().Be("CDF");
            paiement.CodeDevisePrincipale.Should().Be("CDF");
            paiement.MontantPayeDevisePrincipale.Should().Be(100m);

            var tx = await _context.TransactionsMoko.SingleAsync();
            tx.IdPaiement.Should().Be(idPaiement);
            tx.Status.Should().Be(MokoTransactionStatuses.Success);
        }

        [Fact]
        public async Task ConfirmerPayIn_UsdGateway_ConvertsCreditAndPayoutToWalletCdf()
        {
            await using var context = TestDbContextFactory.CreateInMemoryContext();
            context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            context.Ecoles.First().CodeDevisePrincipale = "USD";
            context.EcolesInfoPaiementMobile.Add(new EcoleInfoPaiementMobile
            {
                IdEcole = 1,
                Devise = "CDF",
                MobileMoneyActif = true,
                PayoutAutomatique = true,
                Statut = true,
                DateCreation = DateTime.Now
            });
            context.DevisesMonetaires.AddRange(
                new DeviseMonetaire { IdEcole = 1, CodeDevise = "USD", Libelle = "USD", Statut = true, DateCreation = DateTime.UtcNow },
                new DeviseMonetaire { IdEcole = 1, CodeDevise = "CDF", Libelle = "CDF", Statut = true, DateCreation = DateTime.UtcNow });
            context.TauxChanges.Add(new TauxChange
            {
                IdEcole = 1,
                CodeDeviseSource = "USD",
                CodeDeviseCible = "CDF",
                Taux = 2800m,
                DateEffet = DateTime.UtcNow.AddDays(-1),
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            context.Frais.Add(TestDataBuilder.CreateFrais(1, 1, 1, "Minerval"));
            context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(1, 1, "2025-2026"));
            context.EcolesBeneficiairesMomo.Add(new EcoleBeneficiaireMomo
            {
                IdEcole = 1,
                IdEcoleInfoPaiementMobile = 1,
                Methode = "mpesa",
                Numero = "243800000000",
                NomTitulaire = "Ecole",
                EstPrincipal = true,
                Statut = EcoleBeneficiaireStatuts.Actif,
                DateCreation = DateTime.Now
            });
            context.TransactionsMoko.Add(new TransactionMoko
            {
                IdTransactionMoko = 1,
                Reference = "MOKO_USD_WALLET",
                IdEcole = 1,
                Action = MokoActions.Debit,
                Amount = 10.45m,
                AmountNet = 10m,
                Devise = "USD",
                Method = "mpesa",
                Status = MokoTransactionStatuses.Pending,
                RawRequest = PayInRawRequestHelper.Serialize(
                    new PayInIntentSnapshot
                    {
                        IdEleve = 1,
                        IdFrais = 1,
                        MontantNet = 10m,
                        MontantCollecte = 10.45m,
                        MontantGatewayNet = 10m,
                        CodeDeviseFrais = "USD",
                        CodeDevisePrincipale = "USD",
                        CodeDevisePaiement = "USD",
                        MontantPayeDevisePrincipale = 10m,
                        ModePaiement = "Mobile Money",
                        OperateurMobileMoney = "mpesa"
                    },
                    new Dictionary<string, object?>()),
                DateCreation = DateTime.Now
            });
            await context.SaveChangesAsync();

            // Relier bénéficiaire à l'info créée (Id généré)
            var info = await context.EcolesInfoPaiementMobile.SingleAsync();
            var benef = await context.EcolesBeneficiairesMomo.SingleAsync();
            benef.IdEcoleInfoPaiementMobile = info.IdEcoleInfoPaiementMobile;
            await context.SaveChangesAsync();

            var ecolePaiement = new Mock<IEcolePaiementMobileService>();
            ecolePaiement
                .Setup(s => s.GetBeneficiaireActifAsync(1, It.IsAny<string>()))
                .ReturnsAsync(benef);

            var service = new MokoAfrikaService(
                context,
                Mock.Of<IMokoAfrikaGatewayClient>(),
                Mock.Of<IMokoFeeCalculator>(),
                new MokoWalletService(context, new KelasiNaBiso.Services.CurrencyConversionService(context), NullLogger<MokoWalletService>.Instance),
                ecolePaiement.Object,
                Mock.Of<IPaiementRepository>(),
                Mock.Of<IDashboardHubService>(),
                Options.Create(new MokoSettings { PayoutSettlementDelayMinutes = 3 }),
                NullLogger<MokoAfrikaService>.Instance);

            await service.ConfirmerPayInEtNotifierAsync("MOKO_USD_WALLET", "PD-USD");

            var wallet = await context.EcolesWallets.SingleAsync();
            wallet.Devise.Should().Be("CDF");
            wallet.SoldeEnAttente.Should().Be(28000m);

            var file = await context.FilePayoutsMoko.SingleAsync();
            file.Devise.Should().Be("CDF");
            file.MontantNet.Should().Be(28000m);
        }

        public void Dispose() => _context.Dispose();
    }
}
