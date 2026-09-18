using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
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
    public class PaiementMokoOrchestratorMultiDeviseTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly Mock<IMokoAfrikaGatewayClient> _gateway = new();
        private readonly PaiementMokoOrchestrator _orchestrator;

        public PaiementMokoOrchestratorMultiDeviseTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            SeedGraph();

            _gateway
                .Setup(g => g.SendAsync(It.IsAny<Dictionary<string, object?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MokoGatewayResponse
                {
                    HttpStatusCode = 200,
                    RawBody = "{\"Status\":\"pending\",\"resultCode\":\"0\",\"Transaction_id\":\"PDTEST\"}",
                    IsSuccess = false,
                    IsPending = true,
                    IsFailure = false,
                    Status = "pending",
                    TransactionId = "PDTEST"
                });

            var currency = new CurrencyConversionService(_context);
            var settings = Options.Create(new MokoSettings
            {
                MerchantId = "m",
                MerchantCode = "c",
                SecretKey = "s"
            });

            _orchestrator = new PaiementMokoOrchestrator(
                _context,
                _gateway.Object,
                new MokoFeeCalculator(),
                Mock.Of<IMokoWalletService>(),
                Mock.Of<IEcolePaiementMobileService>(),
                Mock.Of<IMokoAfrikaService>(),
                Mock.Of<IDashboardHubService>(),
                currency,
                settings,
                NullLogger<PaiementMokoOrchestrator>.Instance);
        }

        private void SeedGraph(string codeDevisePrincipale = "USD", string deviseGateway = "CDF", bool withTaux = true)
        {
            _context.Ecoles.Add(new Ecole
            {
                IdEcole = 13,
                Nom = "Ecole FX",
                CodeDevisePrincipale = codeDevisePrincipale,
                Statut = true,
                DateCreation = DateTime.Now
            });
            _context.Directions.Add(new Direction
            {
                IdDirection = 1,
                NomDirection = "Primaire",
                IdEcole = 13,
                Statut = true
            });
            _context.Classes.Add(new Classe
            {
                IdClasse = 1,
                NomClasse = "1ère",
                IdDirection = 1,
                Statut = true
            });
            _context.AnneeScolaires.Add(new AnneeScolaire
            {
                IdAnneeScolaire = 1,
                LibelleAnneeScolaire = "2025-2026",
                IdEcole = 13,
                Statut = true
            });
            _context.Eleves.Add(new Eleve
            {
                IdEleve = 1746,
                Nom = "Test",
                Postnom = "Eleve",
                Prenom = "FX",
                Matricule = "EL-FX-1",
                Nationalite = "CD",
                Statut = true
            });
            _context.Inscriptions.Add(new Inscription
            {
                IdInscription = 1,
                Type = "Inscription",
                IdEleve = 1746,
                IdEcole = 13,
                IdClasse = 1,
                IdAnneeScolaire = 1,
                Statut = true,
                StatutInscription = "Confirmé",
                DateInscription = DateTime.Now
            });
            _context.Frais.Add(new Frais
            {
                IdFrais = 101,
                LibelleFrais = "Minerval",
                Montant = 10,
                Devise = codeDevisePrincipale,
                IdEcole = 13,
                IdAnneeScolaire = 1,
                Portee = PorteeFrais.Direction,
                Statut = true,
                DateCreation = DateTime.Now,
                FraisDirections = new List<FraisDirection>
                {
                    new() { IdFrais = 101, IdDirection = 1 }
                }
            });
            _context.EcolesInfoPaiementMobile.Add(new EcoleInfoPaiementMobile
            {
                IdEcole = 13,
                Devise = deviseGateway,
                MobileMoneyActif = true,
                CarteActif = true,
                Statut = true,
                DateCreation = DateTime.Now
            });

            SeedDevises(_context, 13, codeDevisePrincipale, deviseGateway);

            if (withTaux && !string.Equals(codeDevisePrincipale, deviseGateway, StringComparison.OrdinalIgnoreCase))
            {
                _context.TauxChanges.Add(new TauxChange
                {
                    IdEcole = 13,
                    CodeDeviseSource = codeDevisePrincipale,
                    CodeDeviseCible = deviseGateway,
                    Taux = 2800m,
                    DateEffet = DateTime.UtcNow.AddDays(-1),
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                _context.TauxChanges.Add(new TauxChange
                {
                    IdEcole = 13,
                    CodeDeviseSource = deviseGateway,
                    CodeDeviseCible = codeDevisePrincipale,
                    Taux = 1m / 2800m,
                    DateEffet = DateTime.UtcNow.AddDays(-1),
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
            }

            _context.SaveChanges();

            // Recharger navigations pour Inscription.Classe.Direction
            _context.ChangeTracker.Clear();
        }

        private static void SeedDevises(
            Data.KelasiNaBisoDbContext context,
            int idEcole,
            params string[] codes)
        {
            foreach (var raw in codes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var code = raw.Trim().ToUpperInvariant();
                context.DevisesMonetaires.Add(new DeviseMonetaire
                {
                    IdEcole = idEcole,
                    CodeDevise = code,
                    Libelle = code,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
            }
        }

        private static PayInFraisScolaireRequestDto BaseRequest(string? devise = null, string? currency = null) => new()
        {
            IdEleve = 1746,
            IdFrais = 101,
            MontantNet = 10m,
            Method = "mpesa",
            TelephonePayeur = "243825099299",
            Devise = devise,
            Currency = currency,
            Commentaire = "Test FX"
        };

        [Fact]
        public async Task InitierPayIn_SameDevise_UsesTauxOne_AndGatewayAmountEqualsNet()
        {
            var local = await CreateOrchestratorAsync("CDF", "CDF", "CDF", withTaux: false);
            try
            {
                var result = await local.Orchestrator.InitierPayInFraisScolaireAsync(BaseRequest("CDF"), idUtilisateur: 1);

                result.CodeDevisePrincipale.Should().Be("CDF");
                result.CodeDevisePaiement.Should().Be("CDF");
                result.TauxVersDevisePrincipale.Should().Be(1m);
                result.MontantNet.Should().Be(10m);
                result.MontantPayeDevisePrincipale.Should().Be(10m);

                (await local.Context.Paiements.CountAsync()).Should().Be(0);
                result.IdPaiement.Should().BeNull();

                var tx = await local.Context.TransactionsMoko.SingleAsync();
                tx.Devise.Should().Be("CDF");
                tx.AmountNet.Should().Be(10m);
                tx.IdPaiement.Should().BeNull();
            }
            finally
            {
                await local.Context.DisposeAsync();
            }
        }

        [Fact]
        public async Task InitierPayIn_UsdFee_DefaultSettlement_StaysUsdOnGateway()
        {
            // Sans choix client ≠ frais : gateway = devise du frais (USD), pas force vers MM CDF.
            var result = await _orchestrator.InitierPayInFraisScolaireAsync(BaseRequest(), idUtilisateur: 1);

            result.CodeDevisePrincipale.Should().Be("USD");
            result.CodeDevisePaiement.Should().Be("USD");
            result.MontantNet.Should().Be(10m);
            result.MontantPayeDevisePrincipale.Should().Be(10m);
            result.TauxVersDevisePrincipale.Should().Be(1m);
            result.IdPaiement.Should().BeNull();
            (await _context.Paiements.CountAsync()).Should().Be(0);

            var tx = await _context.TransactionsMoko.SingleAsync();
            tx.Devise.Should().Be("USD");
            tx.AmountNet.Should().Be(10m);
            tx.IdPaiement.Should().BeNull();

            var payload = System.Text.Json.JsonDocument.Parse(tx.RawRequest!);
            payload.RootElement.GetProperty("intent").GetProperty("idEleve").GetInt32().Should().Be(1746);
            payload.RootElement.GetProperty("intent").GetProperty("codeDeviseFrais").GetString().Should().Be("USD");
            payload.RootElement.GetProperty("gateway").GetProperty("currency").GetString().Should().Be("USD");
        }

        [Fact]
        public async Task InitierPayIn_UsdFee_ClientChoosesCdf_ConvertsToGatewayCdf()
        {
            var result = await _orchestrator.InitierPayInFraisScolaireAsync(BaseRequest("CDF"), idUtilisateur: 1);

            result.MontantNet.Should().Be(10m);
            result.CodeDevisePaiement.Should().Be("CDF");
            result.MontantPayeDevisePrincipale.Should().Be(10m);

            var tx = await _context.TransactionsMoko.SingleAsync();
            tx.AmountNet.Should().Be(28000m);
            tx.Devise.Should().Be("CDF");

            var payload = System.Text.Json.JsonDocument.Parse(tx.RawRequest!);
            payload.RootElement.GetProperty("gateway").GetProperty("currency").GetString().Should().Be("CDF");
        }

        [Fact]
        public async Task InitierPayIn_CdfFee_PrincipalUsd_MmCdf_DoesNotTreatAmountAsUsd()
        {
            var local = await CreateOrchestratorAsync(
                principale: "USD",
                gateway: "CDF",
                deviseFrais: "CDF",
                withTaux: true);
            try
            {
                var request = BaseRequest("CDF");
                request.MontantNet = 28000m;

                var result = await local.Orchestrator.InitierPayInFraisScolaireAsync(request, idUtilisateur: 1);

                result.CodeDevisePrincipale.Should().Be("USD");
                result.CodeDevisePaiement.Should().Be("CDF");
                result.MontantNet.Should().Be(28000m);
                result.MontantPayeDevisePrincipale.Should().Be(10m);

                var tx = await local.Context.TransactionsMoko.SingleAsync();
                tx.Devise.Should().Be("CDF");
                tx.AmountNet.Should().Be(28000m);
                (await local.Context.Paiements.CountAsync()).Should().Be(0);
            }
            finally
            {
                await local.Context.DisposeAsync();
            }
        }

        [Fact]
        public async Task InitierPayIn_UsdFee_ExplicitCdfSettlement_ConvertsOnceToGateway()
        {
            var local = await CreateOrchestratorAsync(
                principale: "USD",
                gateway: "CDF",
                deviseFrais: "USD",
                withTaux: true);
            try
            {
                var result = await local.Orchestrator.InitierPayInFraisScolaireAsync(BaseRequest("CDF"), idUtilisateur: 1);

                result.MontantNet.Should().Be(10m);
                result.CodeDevisePaiement.Should().Be("CDF");
                result.MontantPayeDevisePrincipale.Should().Be(10m);

                var tx = await local.Context.TransactionsMoko.SingleAsync();
                tx.AmountNet.Should().Be(28000m);
                tx.Devise.Should().Be("CDF");
            }
            finally
            {
                await local.Context.DisposeAsync();
            }
        }

        [Fact]
        public async Task InitierPayIn_InvalidClientDevise_Throws()
        {
            var act = async () => await _orchestrator.InitierPayInFraisScolaireAsync(
                BaseRequest(devise: "EUR"), idUtilisateur: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*EUR*");
        }

        private static async Task<(PaiementMokoOrchestrator Orchestrator, Data.KelasiNaBisoDbContext Context)> CreateOrchestratorAsync(
            string principale,
            string gateway,
            string deviseFrais,
            bool withTaux)
        {
            var context = TestDbContextFactory.CreateInMemoryContext();
            context.Ecoles.Add(new Ecole
            {
                IdEcole = 13,
                Nom = "Ecole FX",
                CodeDevisePrincipale = principale,
                Statut = true,
                DateCreation = DateTime.Now
            });
            context.Directions.Add(new Direction { IdDirection = 1, NomDirection = "Primaire", IdEcole = 13, Statut = true });
            context.Classes.Add(new Classe { IdClasse = 1, NomClasse = "1ère", IdDirection = 1, Statut = true });
            context.AnneeScolaires.Add(new AnneeScolaire { IdAnneeScolaire = 1, LibelleAnneeScolaire = "2025-2026", IdEcole = 13, Statut = true });
            context.Eleves.Add(new Eleve
            {
                IdEleve = 1746,
                Nom = "Test",
                Postnom = "Eleve",
                Prenom = "FX",
                Matricule = "EL-FX-2",
                Nationalite = "CD",
                Statut = true
            });
            context.Inscriptions.Add(new Inscription
            {
                IdInscription = 1,
                Type = "Inscription",
                IdEleve = 1746,
                IdEcole = 13,
                IdClasse = 1,
                IdAnneeScolaire = 1,
                Statut = true,
                StatutInscription = "Confirmé",
                DateInscription = DateTime.Now
            });
            context.Frais.Add(new Frais
            {
                IdFrais = 101,
                LibelleFrais = "Minerval",
                Montant = 10,
                Devise = deviseFrais,
                IdEcole = 13,
                IdAnneeScolaire = 1,
                Portee = PorteeFrais.Direction,
                Statut = true,
                DateCreation = DateTime.Now,
                FraisDirections = new List<FraisDirection> { new() { IdFrais = 101, IdDirection = 1 } }
            });
            context.EcolesInfoPaiementMobile.Add(new EcoleInfoPaiementMobile
            {
                IdEcole = 13,
                Devise = gateway,
                MobileMoneyActif = true,
                CarteActif = true,
                Statut = true,
                DateCreation = DateTime.Now
            });

            SeedDevises(context, 13, principale, gateway, deviseFrais);

            if (withTaux)
            {
                AddBidirectionalTaux(context, 13, "USD", "CDF", 2800m);
            }

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var gatewayClient = new Mock<IMokoAfrikaGatewayClient>();
            gatewayClient
                .Setup(g => g.SendAsync(It.IsAny<Dictionary<string, object?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MokoGatewayResponse
                {
                    HttpStatusCode = 200,
                    RawBody = "{\"Status\":\"pending\",\"resultCode\":\"0\"}",
                    IsPending = true,
                    TransactionId = "PDTEST2"
                });

            var orchestrator = new PaiementMokoOrchestrator(
                context,
                gatewayClient.Object,
                new MokoFeeCalculator(),
                Mock.Of<IMokoWalletService>(),
                Mock.Of<IEcolePaiementMobileService>(),
                Mock.Of<IMokoAfrikaService>(),
                Mock.Of<IDashboardHubService>(),
                new CurrencyConversionService(context),
                Options.Create(new MokoSettings { MerchantId = "m", MerchantCode = "c", SecretKey = "s" }),
                NullLogger<PaiementMokoOrchestrator>.Instance);

            return (orchestrator, context);
        }

        private static void AddBidirectionalTaux(
            Data.KelasiNaBisoDbContext context,
            int idEcole,
            string codeA,
            string codeB,
            decimal tauxAversB)
        {
            if (string.Equals(codeA, codeB, StringComparison.OrdinalIgnoreCase))
                return;

            context.TauxChanges.Add(new TauxChange
            {
                IdEcole = idEcole,
                CodeDeviseSource = codeA,
                CodeDeviseCible = codeB,
                Taux = tauxAversB,
                DateEffet = DateTime.UtcNow.AddDays(-1),
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            context.TauxChanges.Add(new TauxChange
            {
                IdEcole = idEcole,
                CodeDeviseSource = codeB,
                CodeDeviseCible = codeA,
                Taux = 1m / tauxAversB,
                DateEffet = DateTime.UtcNow.AddDays(-1),
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
        }

        public void Dispose() => _context.Dispose();
    }
}
