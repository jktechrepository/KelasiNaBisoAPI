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
            var local = await CreateOrchestratorAsync("CDF", "CDF", withTaux: false);
            try
            {
                var result = await local.Orchestrator.InitierPayInFraisScolaireAsync(BaseRequest("CDF"), idUtilisateur: 1);

                result.CodeDevisePrincipale.Should().Be("CDF");
                result.CodeDevisePaiement.Should().Be("CDF");
                result.TauxVersDevisePrincipale.Should().Be(1m);
                result.MontantNet.Should().Be(10m);
                result.MontantPayeDevisePrincipale.Should().Be(10m);

                var paiement = await local.Context.Paiements.SingleAsync();
                paiement.CodeDevisePaiement.Should().Be("CDF");
                paiement.MontantNet.Should().Be(10m);

                var tx = await local.Context.TransactionsMoko.SingleAsync();
                tx.Devise.Should().Be("CDF");
                tx.AmountNet.Should().Be(10m);
            }
            finally
            {
                await local.Context.DisposeAsync();
            }
        }

        [Fact]
        public async Task InitierPayIn_UsdToCdf_ConvertsGatewayAmount_AndSnapshotsPrincipal()
        {
            var result = await _orchestrator.InitierPayInFraisScolaireAsync(BaseRequest("USD"), idUtilisateur: 1);

            result.CodeDevisePrincipale.Should().Be("USD");
            result.CodeDevisePaiement.Should().Be("CDF");
            result.MontantNet.Should().Be(10m);
            result.MontantPayeDevisePrincipale.Should().Be(10m);
            result.TauxVersDevisePrincipale.Should().BeApproximately(1m / 2800m, 0.0000001m);

            var tx = await _context.TransactionsMoko.SingleAsync();
            tx.Devise.Should().Be("CDF");
            tx.AmountNet.Should().Be(28000m);

            var payload = System.Text.Json.JsonDocument.Parse(tx.RawRequest!);
            payload.RootElement.GetProperty("currency").GetString().Should().Be("CDF");
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
                Devise = principale,
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

            SeedDevises(context, 13, principale, gateway);

            if (withTaux && !string.Equals(principale, gateway, StringComparison.OrdinalIgnoreCase))
            {
                context.TauxChanges.Add(new TauxChange
                {
                    IdEcole = 13,
                    CodeDeviseSource = principale,
                    CodeDeviseCible = gateway,
                    Taux = 2800m,
                    DateEffet = DateTime.UtcNow.AddDays(-1),
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
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

        public void Dispose() => _context.Dispose();
    }
}
